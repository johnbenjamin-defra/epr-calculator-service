namespace EPR.Calculator.Service.Common.Logging
{
    using System;

    /// <summary>
    /// Interface for logging telemetry data for calculator operations.
    /// </summary>
    public interface ICalculatorTelemetryLogger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="runId">The unique identifier for the run.</param>
        /// <param name="runName">The name of the run.</param>
        /// <param name="message">The message to log.</param>
        void LogInformation(string runId, string runName, string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="runId">The unique identifier for the run.</param>
        /// <param name="runName">The name of the run.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">The exception to log.</param>
        void LogError(string runId, string runName, string message, Exception ex);
    }
}