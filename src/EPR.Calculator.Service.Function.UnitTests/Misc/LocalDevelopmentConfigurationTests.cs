namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using System;
    using System.Configuration;
    using AutoFixture;
    using Castle.Core.Configuration;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LocalDevelopmentConfigurationTests
    {
        private LocalDevelopmentConfiguration TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private Mock<Microsoft.Extensions.Configuration.IConfiguration> Configuration { get; init; }

        public LocalDevelopmentConfigurationTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            this.TestClass = new LocalDevelopmentConfiguration(this.Configuration.Object);
        }

        [TestMethod]
        public void CanGetCheckInterval()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.CheckInterval, typeof(string));
        }

        [TestMethod]
        public void CanGetDbConnectionString()
        {
            // Arrange
            var blobConnectionValue = this.Fixture.Create<string>();
            var blobConnectionSection = new Mock<IConfigurationSection>();
            blobConnectionSection.Setup(v => v.Value).Returns(blobConnectionValue);
            this.Configuration.Setup(c => c.GetSection("DbConnectionString"))
                .Returns(blobConnectionSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.DbConnectionString, typeof(string));
        }

        [TestMethod]
        public void CanGetExecuteRPDPipeline()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.ExecuteRPDPipeline, typeof(string));
        }

        [TestMethod]
        public void CanGetMaxCheckCount()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.MaxCheckCount, typeof(string));
        }

        [TestMethod]
        public void CanGetOrgDataPipelineName()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.OrgDataPipelineName, typeof(string));
        }

        [TestMethod]
        public void CanGetPipelineUrl()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PipelineUrl, typeof(string));
        }

        [TestMethod]
        public void CanGetPomDataPipelineName()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PomDataPipelineName, typeof(string));
        }

        [TestMethod]
        public void CanGetPrepareCalcResultEndPoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PrepareCalcResultEndPoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetPrepareCalcResultsTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PrepareCalcResultsTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetRpdStatusTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.RpdStatusTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetStatusEndpoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.StatusEndpoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetTransposeEndpoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.TransposeEndpoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetTransposeTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.TransposeTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetBlobConnectionString()
        {
            // Arrange
            var blobConnectionValue = this.Fixture.Create<string>();
            var blobConnectionSection = new Mock<IConfigurationSection>();
            blobConnectionSection.Setup(v => v.Value).Returns(blobConnectionValue);
            this.Configuration.Setup(c => c.GetSection("BlobConnectionString"))
                .Returns(blobConnectionSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.BlobConnectionString, typeof(string));
        }

        [TestMethod]
        public void CanGetDbLoadingChunkSize()
        {
            // Arrange
            var dbLoadingChunkSize = this.Fixture.Create<int>();
            var blobConnectionSection = new Mock<IConfigurationSection>();
            blobConnectionSection.Setup(v => v.Value).Returns(dbLoadingChunkSize.ToString());
            this.Configuration.Setup(c => c.GetSection(EnvironmentVariableKeys.DbLoadingChunkSize))
                .Returns(blobConnectionSection.Object);

            // Assert
            Assert.IsInstanceOfType(this.TestClass.DbLoadingChunkSize, typeof(int));
        }
    }
}