namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.ApplicationInsights;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Inserts records to the database, divided into chunks to avoid timeouts
    /// from inserting large numbers of records at once.
    /// </summary>
    /// <typeparam name="TRecord">The type of the records to be inserted.</typeparam>
    /// <param name="telemetryClient">A <see cref="telemetryClient"/> for outputting debug info to Azure Insights.</param>
    /// <param name="context">The database context.</param>
    /// <param name="table">The database table to load the data into.</param>
    /// <param name="chunkSize">The number of records to load in each chunk.</param>
    public class DbLoadingChunkerService<TRecord> : IDbLoadingChunkerService<TRecord>
        where TRecord : class
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerService{TRecord}"/> class.
        /// </summary>
        /// <param name="config">The application config settings.</param>
        /// <param name="telemetryClient">A telemetry client.</param>
        /// <param name="commandTimeoutService">A service to set the database command timeout.</param>
        /// <param name="context">The database context.</param>
        /// <param name="table">The database table to insert data into.</param>
        [ActivatorUtilitiesConstructor]
        public DbLoadingChunkerService(
            IConfigurationService config,
            TelemetryClient telemetryClient,
            ICommandTimeoutService commandTimeoutService,
            ApplicationDBContext context)
            : this(telemetryClient, commandTimeoutService, context, config.DbLoadingChunkSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerService{TRecord}"/> class.
        /// </summary>
        /// <param name="telemetryClient">A telemetry client.</param>
        /// /// <param name="commandTimeoutService">A service to set the database command timeout.</param>
        /// <param name="context">The database context.</param>
        /// <param name="table">The database table to insert data into.</param>
        /// <param name="chunkSize">The number of records to include in each chunk of records.</param>
        public DbLoadingChunkerService(
            TelemetryClient telemetryClient,
            ICommandTimeoutService commandTimeoutService,
            ApplicationDBContext context,
            int chunkSize)
        {
            this.TelemetryClient = telemetryClient;
            this.Context = context;
            this.Table = context.Set<TRecord>();
            this.ChunkSize = chunkSize;

            commandTimeoutService.SetCommandTimeout(context.Database);
        }

        private int ChunkSize { get; init; }

        private DbContext Context { get; set; }

        private TelemetryClient TelemetryClient { get; init; }

        private DbSet<TRecord> Table { get; set; }

        /// <inheritdoc/>
        public async Task InsertRecords(IEnumerable<TRecord> records)
        {
            Console.WriteLine($"Loading {typeof(TRecord).Name} records in chunks of {this.ChunkSize}.");
            this.TelemetryClient.TrackTrace($"Loading {typeof(TRecord).Name} records in chunks of {this.ChunkSize}.");
            var chunkContents = new List<TRecord>();
            var recordCount = 1;
            var chunkCount = 1;
            var currentChunkRecordCount = 1;

            var totalTimer = new Stopwatch();
            var chunkTimer = new Stopwatch();
            totalTimer.Start();
            chunkTimer.Start();

            foreach (var record in records)
            {
                chunkContents.Add(record);
                if (chunkContents.Count >= this.ChunkSize)
                {
                    await this.SaveChunk(chunkCount, chunkTimer, chunkContents);
                    chunkContents.Clear();
                    chunkCount++;
                }

                recordCount++;
                currentChunkRecordCount++;
            }

            await this.SaveChunk(chunkCount, chunkTimer, chunkContents);

            totalTimer.Stop();
            Console.WriteLine($"Total time taken to insert chunks: {totalTimer.Elapsed}");
            this.TelemetryClient.TrackTrace($"Total time taken to insert chunks: {totalTimer.Elapsed}");
        }

        private async Task SaveChunk(int chunkCount, Stopwatch chunkTimer, IEnumerable<TRecord> chunkBuffer)
        {
            this.Table.AddRange(chunkBuffer);
            await this.Context.SaveChangesAsync();

            chunkTimer.Stop();
            Console.WriteLine($"Time to insert chunk {chunkCount}: {chunkTimer.Elapsed}");
            this.TelemetryClient.TrackTrace($"Time to insert chunk {chunkCount}: {chunkTimer.Elapsed}");
            chunkTimer.Restart();
        }
    }
}
