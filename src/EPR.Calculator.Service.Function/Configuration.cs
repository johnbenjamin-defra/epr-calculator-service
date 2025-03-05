// <copyright file="Configuration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Service.Function
{
    using System;
    using System.Globalization;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Interface;

    /// <summary>
    /// Provides configuration settings for the calculator service.
    /// </summary>
    public class Configuration : IConfigurationService
    {
        /// <summary>
        /// The default value for calculator run and transpose timeouts.
        /// </summary>
        public const double DefaultTimeout = 24;

        /// <summary>
        /// Gets the pipeline URL from environment variables.
        /// </summary>
        public string PipelineUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl);

        /// <summary>
        /// Gets the organization data pipeline name from environment variables.
        /// </summary>
        public string OrgDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName);

        /// <summary>
        /// Gets the POM data pipeline name from environment variables.
        /// </summary>
        public string PomDataPipelineName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName);

        /// <summary>
        /// Gets the check interval from environment variables.
        /// </summary>
        public string CheckInterval => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CheckInterval);

        /// <summary>
        /// Gets the maximum check count from environment variables.
        /// </summary>
        public string MaxCheckCount => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.MaxCheckCount);

        /// <summary>
        /// Gets the status update endpoint URI from environment variables.
        /// </summary>
        public Uri StatusEndpoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint));

        /// <summary>
        /// Gets the URI for the endpoint used to prepare calculation results from environment variables.
        /// </summary>
        public Uri PrepareCalcResultEndPoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultEndPoint));

        /// <summary>
        /// Gets the URI for the endpoint used to Transpose before calculation of results from environment variables.
        /// </summary>
        public Uri TransposeEndpoint => new Uri(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TransposeEndpoint));

        /// <summary>
        /// Gets the flag indicating whether to execute the RPD pipeline from environment variables.
        /// </summary>
        public string ExecuteRPDPipeline => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.ExecuteRPDPipeline);

        /// <summary>
        /// Gets the RPD status timeout from environment variables.
        /// </summary>
        public TimeSpan RpdStatusTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RpdStatusTimeout));

        /// <summary>
        /// Gets the calculator run timeout from environment variables.
        /// </summary>
        public TimeSpan PrepareCalcResultsTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.PrepareCalcResultsTimeout));

        /// <summary>
        /// Gets the transpose timeout from environment variables.
        /// </summary>
        public TimeSpan TransposeTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TransposeTimeout));

        public string DbConnectionString => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.DbConnectionString);

        /// <inheritdoc/>
        public string BlobContainerName => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.BlobContainerName);
 
        public string BlobConnectionString => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.BlobConnectionString);

        /// <inheritdoc/>
        public TimeSpan CommandTimeout => ParseTimeSpan(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.CommandTimeout));

        public int DbLoadingChunkSize => int.Parse(
            Environment.GetEnvironmentVariable(EnvironmentVariableKeys.DbLoadingChunkSize) ?? "1000");

        private static TimeSpan ParseTimeSpan(string value)
        {
            if (double.TryParse(value, out double timeout))
            {
                return TimeSpan.FromMinutes(timeout);
            }

            return TimeSpan.FromHours(DefaultTimeout);
        }
    }
}
