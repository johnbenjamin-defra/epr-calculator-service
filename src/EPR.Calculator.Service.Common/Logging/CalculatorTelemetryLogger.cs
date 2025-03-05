namespace EPR.Calculator.Service.Common.Logging
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Logger for telemetry data in the calculator service.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CalculatorTelemetryLogger"/> class.
    /// </remarks>
    /// <param name="telemetryClient">The telemetry client to use for logging.</param>
    public class CalculatorTelemetryLogger : ICalculatorTelemetryLogger
    {
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorTelemetryLogger"/> class.
        /// </summary>
        /// <param name="telemetryClient">The telemetry client to use for logging.</param>
        public CalculatorTelemetryLogger(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="runId">The run identifier.</param>
        /// <param name="runName">The run name.</param>
        /// <param name="message">The message to log.</param>
        public void LogInformation(string runId, string runName, string message)
        {
            this.TrackTrace(runId, runName, message, SeverityLevel.Information);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="runId">The run identifier.</param>
        /// <param name="runName">The run name.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">The exception to log.</param>
        public void LogError(string runId, string runName, string message, Exception ex)
        {
            var telemetry = new ExceptionTelemetry(ex)
            {
                SeverityLevel = SeverityLevel.Error,
                Message = CreateLogMessage(runId, runName, message),
            };
            this.telemetryClient.TrackException(telemetry);
        }

        private static string CreateLogMessage(string runId, string runName, string message)
        {
            return $"[{DateTime.Now}] RunId: {runId}, RunName: {runName}, Message: {message}";
        }

        private void TrackTrace(string runId, string runName, string message, SeverityLevel severityLevel)
        {
            var telemetry = new TraceTelemetry
            {
                Message = CreateLogMessage(runId, runName, message),
                SeverityLevel = severityLevel,
            };
            this.telemetryClient.TrackTrace(telemetry);
        }
    }
}