namespace EPR.Calculator.Service.Function.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Inserts records to the database, divided into chunks to avoid timeouts
    /// from inserting large numbers of records at once.
    /// </summary>
    /// <typeparam name="TRecord">The type of the records to be inserted.</typeparam>
    public interface IDbLoadingChunkerService<in TRecord>
        where TRecord : class
    {
        /// <summary>
        /// Insert a collection of records into the database using chunks.
        /// </summary>
        /// <param name="records">The collection of records to insert.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task InsertRecords(IEnumerable<TRecord> records);
    }
}