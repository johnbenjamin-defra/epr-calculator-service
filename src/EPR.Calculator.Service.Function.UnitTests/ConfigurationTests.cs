// <copyright file="ConfigurationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function;
    using EPR.Calculator.Service.Function.Constants;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Contains tests for configuration variables.
    /// </summary>
    [TestClass]
    public class ConfigurationTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTests"/> class.
        /// </summary>
        public ConfigurationTests() => this.Fixture = new Fixture();

        private Fixture Fixture { get; init; }

        /// <summary>
        /// Tests that the configuration variables match the expected environment variables.
        /// </summary>
        [TestMethod]
        public void Configuration_Variables_Test()
        {
            // Arrange
            var configuration = new Configuration();

            // Assert
            Assert.AreEqual(Environment.GetEnvironmentVariable("PipelineUrl"), configuration.PipelineUrl);
            Assert.AreEqual(Environment.GetEnvironmentVariable("GetOrgDataPipelineName"), configuration.OrgDataPipelineName);
            Assert.AreEqual(Environment.GetEnvironmentVariable("GetPomDataPipelineName"), configuration.PomDataPipelineName);
            Assert.AreEqual(Environment.GetEnvironmentVariable("CheckInterval"), configuration.CheckInterval);
            Assert.AreEqual(Environment.GetEnvironmentVariable("MaxCheckCount"), configuration.MaxCheckCount);
            Assert.AreEqual(Environment.GetEnvironmentVariable("ExecuteRPDPipeline"), configuration.ExecuteRPDPipeline);
        }

        /// <summary>
        /// Checks that the calculator run timeout can be retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetPrepareCalcResultsTimeout()
        {
            // Arrange
            var testValueInMinutes = this.Fixture.Create<double>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultsTimeout,
                testValueInMinutes.ToString());

            // Act
            var result = new Configuration().PrepareCalcResultsTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(testValueInMinutes), result);
        }

        /// <summary>
        /// Checks that when no value has been set for the calculator run timeout,
        /// the default value is retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetDefaultPrepareCalcResultsTimeout()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultsTimeout,
                null);

            // Act
            var result = new Configuration().PrepareCalcResultsTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(Configuration.DefaultTimeout), result);
        }

        /// <summary>
        /// Checks that the rpd status timeout can be retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetRpdStatusTimeout()
        {
            // Arrange
            var testValueInMinutes = this.Fixture.Create<double>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.RpdStatusTimeout,
                testValueInMinutes.ToString());

            // Act
            var result = new Configuration().RpdStatusTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(testValueInMinutes), result);
        }

        /// <summary>
        /// Checks that when no value has been set for the rpd status timeout,
        /// the default value is retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetDefaultRpdStatusTimeout()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.RpdStatusTimeout,
                null);

            // Act
            var result = new Configuration().RpdStatusTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(Configuration.DefaultTimeout), result);
        }

        /// <summary>
        /// Checks that the calculator run timeout can be retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetTransposeTimeout()
        {
            // Arrange
            var testValueInMinutes = this.Fixture.Create<double>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.TransposeTimeout,
                testValueInMinutes.ToString());

            // Act
            var result = new Configuration().TransposeTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(testValueInMinutes), result);
        }

        /// <summary>
        /// Checks that when no value has been set for the calculator run timeout,
        /// the default value is retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetDefaultTransposeTimeout()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.TransposeTimeout,
                null);

            // Act
            var result = new Configuration().TransposeTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(Configuration.DefaultTimeout), result);
        }

        [TestMethod]
        public void CanGetDbConnectionString()
        {
            // Arrange
            var connectionString = this.Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.DbConnectionString,
                connectionString);

            // Act
            var result = new Configuration().DbConnectionString;

            // Assert
            Assert.AreEqual(connectionString, result);
        }

        [TestMethod]
        public void CanGetTransposeEndpoint()
        {
            // Arrange
            var transposeEndpoint = this.Fixture.Create<Uri>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.TransposeEndpoint,
                transposeEndpoint.ToString());

            // Act
            var result = new Configuration().TransposeEndpoint;

            // Assert
            Assert.AreEqual(transposeEndpoint, result);
        }

        [TestMethod]
        public void CanGetDbLoadingChunkSize()
        {
            // Arrange
            var dbLoadingChunkSize = this.Fixture.Create<int>();

            Environment.SetEnvironmentVariable(
               EnvironmentVariableKeys.DbLoadingChunkSize,
               dbLoadingChunkSize.ToString());

            // Act
            var result = new Configuration().DbLoadingChunkSize;

            // Assert
            Assert.AreEqual(dbLoadingChunkSize, result);
        }
    }
}