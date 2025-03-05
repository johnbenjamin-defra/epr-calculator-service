namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TRecord = System.String;

    /// <summary>Unit tests for <see cref="DbLoadingChunkerService{TRecord}"/>.</summary>
    [TestClass]
    public class DbLoadingChunkerServiceTests
    {
        private const int ChunkSize = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbLoadingChunkerServiceTests"/> class.
        /// </summary>
        public DbLoadingChunkerServiceTests()
        {
            this.Fixture = new Fixture();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            this.Context = new ApplicationDBContext(dbContextOptions);

            this.ProducerDetailTestClass = new DbLoadingChunkerService<ProducerDetail>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                this.Context,
                ChunkSize);

            this.ProducerReportedMaterialTestClass = new DbLoadingChunkerService<ProducerReportedMaterial>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                this.Context,
                ChunkSize);
        }

        private DbLoadingChunkerService<ProducerDetail> ProducerDetailTestClass { get; set; }

        private DbLoadingChunkerService<ProducerReportedMaterial> ProducerReportedMaterialTestClass { get; set; }

        private IFixture Fixture { get; init; }

        private ApplicationDBContext Context { get; set; }

        /// <summary>
        /// Checks that the <see cref="DbLoadingChunkerService{TRecord}.InsertRecords"/> method inserts 
        /// the expected number of records.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task CanCallInsertRecords()
        {
            // Arrange
            var numberOfRecords = 100;
            var records = this.Fixture.CreateMany<ProducerDetail>(numberOfRecords);

            // Act
            await this.ProducerDetailTestClass.InsertRecords(records);

            // Assert
            Assert.AreEqual(numberOfRecords, await this.Context.ProducerDetail.CountAsync());
        }

        /// <summary>
        /// Checks the insert of a producer detail record.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Detail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 1,
                ProducerName = "UPU LIMITED",
                CalculatorRunId = 1,
                CalculatorRun = this.Fixture.Create<CalculatorRun>(),
            };

            await this.ProducerDetailTestClass.InsertRecords([expectedResult]);

            var producerDetail = await this.Context.ProducerDetail.FirstOrDefaultAsync();
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        /// <summary>
        /// Checks the insert of a producer reported material record.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Reported_Material()
        {
            var expectedResult = new ProducerReportedMaterial
            {
                Id = 1,
                MaterialId = 4,
                ProducerDetailId = 1,
                PackagingType = "CW",
                PackagingTonnage = 1,
                Material = new Material
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                ProducerDetail = new ProducerDetail
                {
                    Id = 1,
                    ProducerId = 1,
                    SubsidiaryId = "1",
                    ProducerName = "UPU LIMITED",
                    CalculatorRunId = 1,
                    CalculatorRun = this.Fixture.Create<CalculatorRun>(),
                },
            };
            await this.ProducerReportedMaterialTestClass.InsertRecords([expectedResult]);

            var producerReportedMaterial = await this.Context.ProducerReportedMaterial.FirstOrDefaultAsync();
            Assert.IsNotNull(producerReportedMaterial);
            Assert.AreEqual(expectedResult.Material.Code, producerReportedMaterial.Material!.Code);
            Assert.AreEqual(expectedResult.Material.Name, producerReportedMaterial.Material.Name);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerId, producerReportedMaterial.ProducerDetail!.ProducerId);
            Assert.AreEqual(expectedResult.ProducerDetail.ProducerName, producerReportedMaterial.ProducerDetail.ProducerName);
        }

        /// <summary>
        /// Checks the insert of a producer subsidary record.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Subsidary_Detail()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 2,
                SubsidiaryId = "1",
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = this.Fixture.Create<CalculatorRun>(),
            };

            await this.ProducerDetailTestClass.InsertRecords([expectedResult]);

            var producerDetail = await this.Context.ProducerDetail.FirstOrDefaultAsync(t => t.SubsidiaryId != null);
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        /// <summary>
        /// Checks the insert of a producer detail record without a submission period.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task Transpose_Should_Return_Correct_Producer_Detail_When_Submission_Period_Not_Exists()
        {
            var expectedResult = new ProducerDetail
            {
                Id = 1,
                ProducerId = 2,
                ProducerName = "Subsid2",
                CalculatorRunId = 1,
                CalculatorRun = this.Fixture.Create<CalculatorRun>(),
            };

            await this.ProducerDetailTestClass.InsertRecords([expectedResult]);

            var producerDetail = await this.Context.ProducerDetail.FirstOrDefaultAsync();
            Assert.IsNotNull(producerDetail);
            Assert.AreEqual(expectedResult.ProducerId, producerDetail.ProducerId);
            Assert.AreEqual(expectedResult.ProducerName, producerDetail.ProducerName);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new DbLoadingChunkerService<string>(
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                this.Context,
                this.Fixture.Create<int>());

            // Assert
            Assert.IsNotNull(instance);

            // Act
            instance = new DbLoadingChunkerService<string>(
                new Mock<IConfigurationService>().Object,
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                new Mock<ICommandTimeoutService>().Object,
                this.Context);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}