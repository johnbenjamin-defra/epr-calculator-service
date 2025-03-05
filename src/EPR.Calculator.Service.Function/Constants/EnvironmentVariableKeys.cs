// <copyright file="EnvironmentVariableKeys.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace EPR.Calculator.Service.Function.Constants
{
    /// <summary>
    /// Contains keys for environment variables used in the calculator service.
    /// </summary>
    public static class EnvironmentVariableKeys
    {
        /// <summary>The URL of the synapse pipeline.</summary>
        public const string PipelineUrl = "PipelineUrl";

        /// <summary>The name of the organization data pipeline.</summary>
        public const string OrgDataPipelineName = "GetOrgDataPipelineName";

        /// <summary>The name of the POM data pipeline.</summary>
        public const string PomDataPipelineName = "GetPomDataPipelineName";

        /// <summary>The interval for checking the pipeline status.</summary>
        public const string CheckInterval = "CheckInterval";

        /// <summary>The maximum number of checks to perform.</summary>
        public const string MaxCheckCount = "MaxCheckCount";

        /// <summary>The URL of the endpoint called to update the pipeline status in the database.</summary>
        public const string StatusUpdateEndpoint = "StatusUpdateEndpoint";

        /// <summary> The URL for the endpoint called to Transpose before the Calculation.</summary>
        public const string TransposeEndpoint = "TransposeEndpoint";

        /// <summary>The URL of the endpoint called to prepare the result file in blob storage.</summary>
        public const string PrepareCalcResultEndPoint = "PrepareCalcResultEndPoint";

        /// <summary>The key for executing the RPD pipeline.</summary>
        public const string ExecuteRPDPipeline = "ExecuteRPDPipeline";

        /// <summary>The key for the RpdStatusTimeout environment variable.</summary>
        public const string RpdStatusTimeout = "RpdStatusTimeout";

        /// <summary>The key for the CalculatorRunTimeout environment variable.</summary>
        public const string PrepareCalcResultsTimeout = "PrepareCalcResultsTimeout";

        /// <summary>The key for the TransposeTimeout environment variable.</summary>
        public const string TransposeTimeout = "TransposeTimeout";

        /// <summary>The key for the DbConnectionString environment variable.</summary>
        public const string DbConnectionString = "DbConnectionString";

        /// <summary>The key for the BlobContainerName environment variable.</summary>
        public const string BlobContainerName = "BlobContainerName";

        /// <summary>The key for the BlobConnectionString environment variable.</summary>
        public const string BlobConnectionString = "BlobConnectionString";

        /// <summary>The key for the CommandTimout environment variable.</summary>
        public const string CommandTimeout = "CommandTimeout";

        /// <summary>The key for the InstrumentationKey environment variable.</summary>
        public const string InstrumentationKey = "InstrumentationKey";
    }
}
