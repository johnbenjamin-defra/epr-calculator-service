namespace EPR.Calculator.Service.Function.Misc
{
    using System;
    using System.Configuration;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.Extensions.Configuration;

    public class LocalDevelopmentConfiguration : IConfigurationService
    {
        public LocalDevelopmentConfiguration(IConfiguration configuration) => this.Configuration = configuration;

        /// <summary>
        /// Store the configuration file that's passed using dependancy injection.
        /// </summary>
        private IConfiguration Configuration { get; }

        public string CheckInterval => string.Empty;

        public string DbConnectionString
            => Configuration.GetValue("DbConnectionString", string.Empty);

        public string ExecuteRPDPipeline => string.Empty;

        public string MaxCheckCount => string.Empty;

        public string OrgDataPipelineName => string.Empty;

        public string PipelineUrl => string.Empty;

        public string PomDataPipelineName => string.Empty;

        public Uri PrepareCalcResultEndPoint => new Uri("http://localhost:5055/v1/internal/prepareCalcResults");

        public TimeSpan PrepareCalcResultsTimeout => TimeSpan.FromDays(1);

        public TimeSpan RpdStatusTimeout => TimeSpan.FromDays(1);

        public Uri StatusEndpoint => new Uri("http://localhost:5055/v1/internal/rpdStatus");

        public Uri TransposeEndpoint => new Uri("http://localhost/v1/internal/transposeBeforeCalcResults");

        public TimeSpan TransposeTimeout => TimeSpan.FromDays(1);

        /// <inheritdoc/>
        public string BlobContainerName => Configuration.GetValue("BlobContainerName", string.Empty);

        public string BlobConnectionString
            => Configuration.GetValue("BlobConnectionString", string.Empty);

        public string InstrumentationKey => Configuration.GetValue("InstrumentationKey", string.Empty);

        public TimeSpan CommandTimeout
        {
            get
            {
                double.TryParse(
                    this.Configuration.GetValue("CommandTimeout", string.Empty),
                    out double timeoutInMinutes);
                return TimeSpan.FromMinutes(timeoutInMinutes);
            }
        }
    }
}
