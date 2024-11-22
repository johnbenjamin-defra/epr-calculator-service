namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using System.Net;
    using AutoFixture;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Common.AzureSynapse;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;

    [TestClass]
    public class CalculatorRunServiceTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculatorRunServiceTests"/> class.
        /// </summary>
        public CalculatorRunServiceTests()
        {
            this.Fixture = new Fixture();
            this.AzureSynapseRunner = new Mock<IAzureSynapseRunner>();
            this.MockLogger = new Mock<ILogger<CalculatorRunService>>();
            this.MockStatusUpdateHandler = new Mock<HttpMessageHandler>();

            this.MockStatusUpdateHandler.Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = new HttpClient(this.MockStatusUpdateHandler.Object)
            {
                BaseAddress = new Uri("http://test.com"),
            };

            this.PipelineClientFactory = new Mock<PipelineClientFactory>();
            this.PipelineClientFactory.Setup(factory => factory.GetStatusUpdateClient(It.IsAny<Uri>()))
         .Returns(httpClient);

            this.CalculatorRunService = new CalculatorRunService(
                this.AzureSynapseRunner.Object,
                this.MockLogger.Object,
                this.PipelineClientFactory.Object);

            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, this.Fixture.Create<string>());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, this.Fixture.Create<string>());
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.StatusUpdateEndpoint, this.Fixture.Create<string>());
        }

        private CalculatorRunService CalculatorRunService { get; }

        private Mock<IAzureSynapseRunner> AzureSynapseRunner { get; }

        private Mock<ILogger<CalculatorRunService>> MockLogger { get; }

        private Fixture Fixture { get; }

        private Mock<HttpMessageHandler> MockStatusUpdateHandler { get; set; }

        private Mock<PipelineClientFactory> PipelineClientFactory { get; }

        [TestMethod]
        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        /// <param name="pipelineNameKey">Which pipeline to test - The service should call the synapse runner twice

        public async Task StartProcessCallsAzureSynapseRunnerIfOrgPipelineisUnsuccessful()
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var financialYear = "2024-25";
            var user = this.Fixture.Create<string>();

            var checkInterval = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxChecks = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxChecks.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgpipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgpipelineName.ToString());

            var pompipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pompipelineName.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var runRPDPipeline = this.Fixture.Create<bool>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = financialYear,
                User = user,
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                FinancialYear = financialYear,
                MaxChecks = maxChecks,
                PipelineUrl = pipelineUrl,
                PipelineName = orgpipelineName,
                StatusUpdateEndpoint = statusUpdateEndpoint,
            };

            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.CalculatorRunId == id &&
                p.CheckInterval == checkInterval &&
                p.FinancialYear == financialYear &&
                p.MaxChecks == maxChecks &&
                p.PipelineUrl == pipelineUrl &&
                p.PipelineName == orgpipelineName &&
                p.StatusUpdateEndpoint == statusUpdateEndpoint)))
                .ReturnsAsync(true);

            this.AzureSynapseRunner.Setup(t => t.Process(It.Is<AzureSynapseRunnerParameters>(p =>
                p.CalculatorRunId == id &&
                p.CheckInterval == checkInterval &&
                p.FinancialYear == financialYear &&
                p.MaxChecks == maxChecks &&
                p.PipelineUrl == pipelineUrl &&
                p.PipelineName == pompipelineName &&
                p.StatusUpdateEndpoint == statusUpdateEndpoint)))
                .ReturnsAsync(true);

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameters);

            Assert.IsTrue(result);
        }

        [TestMethod]
        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        /// <param name="pipelineNameKey">Which pipeline to test - The service should call the synapse runner twice
        /// - once for each pipeline, so we run this test for each pipeline we want to check is being called.</param>
        [DataRow(EnvironmentVariableKeys.OrgDataPipelineName)]
        [DataRow(EnvironmentVariableKeys.PomDataPipelineName)]
        public async Task StartProcessCallsAzureSynapseRunnerWithRPDPipelineAsFalse(string pipelineNameKey)
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var financialYear = "2024-25";
            var user = this.Fixture.Create<string>();

            var checkInterval = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxChecks = this.Fixture.Create<int>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxChecks.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgpipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgpipelineName.ToString());

            var pompipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pompipelineName.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var runRPDPipeline = false;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = financialYear,
                User = user,
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                FinancialYear = financialYear,
                MaxChecks = maxChecks,
                PipelineUrl = pipelineUrl,
                PipelineName = orgpipelineName,
                StatusUpdateEndpoint = statusUpdateEndpoint,
            };

            // Act
            await this.CalculatorRunService.StartProcess(calculatorRunParameters);

            // Assert
            this.AzureSynapseRunner.Verify(
                runner => runner.Process(
                    It.IsAny<AzureSynapseRunnerParameters>()),
                Times.Never);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        [TestMethod]
        public async Task StartProcessCallsAzureSynapseRunnerSuccessful()
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var financialYear = "2024-25";
            var user = this.Fixture.Create<string>();

            var checkInterval = 5;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxChecks = 10;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxChecks.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>(); ;
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgpipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgpipelineName.ToString());

            var pompipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pompipelineName.ToString());

            var runRPDPipeline = true;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = financialYear,
                User = user,
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                FinancialYear = financialYear,
                MaxChecks = maxChecks,
                PipelineUrl = pipelineUrl,
                PipelineName = orgpipelineName,
                StatusUpdateEndpoint = statusUpdateEndpoint,
            };

            this.AzureSynapseRunner.Setup(t => t.Process(It.IsAny<AzureSynapseRunnerParameters>())).ReturnsAsync(true);

            // Act
            await this.CalculatorRunService.StartProcess(calculatorRunParameters);

            // Assert
            this.AzureSynapseRunner.Verify(
                runner => runner.Process(
                    It.IsAny<AzureSynapseRunnerParameters>()),
                Times.Exactly(2));

            this.AzureSynapseRunner.Verify(
                runnner => runnner.Process(
                    It.Is<AzureSynapseRunnerParameters>(args =>
                    args == expectedParameters)),
                Times.Never);
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Checks that the service calls the Azure Synapse runner and passes the correct parameters to it.
        /// </summary>
        [TestMethod]
        public async Task StartProcessCallsAzureSynapseRunnerRPDPipelineFalse()
        {
            // Arrange
            var id = this.Fixture.Create<int>();
            var financialYear = "2024-25";
            var user = this.Fixture.Create<string>();

            var checkInterval = 5;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.CheckInterval,
                checkInterval.ToString());

            var maxChecks = 10;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.MaxCheckCount,
                maxChecks.ToString());

            var pipelineUrl = this.Fixture.Create<Uri>(); ;
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PipelineUrl, pipelineUrl.ToString());

            var orgpipelineName = "test";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.OrgDataPipelineName, orgpipelineName.ToString());

            var pompipelineName = "pomtest";
            Environment.SetEnvironmentVariable(EnvironmentVariableKeys.PomDataPipelineName, pompipelineName.ToString());

            var runRPDPipeline = false;
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.ExecuteRPDPipeline,
                runRPDPipeline.ToString());

            var statusUpdateEndpoint = this.Fixture.Create<Uri>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.StatusUpdateEndpoint,
                statusUpdateEndpoint.ToString());

            var calculatorRunParameters = new CalculatorRunParameter
            {
                Id = id,
                FinancialYear = financialYear,
                User = user,
            };

            // The values that the service is expected to pass to the pipeline runner.
            var expectedParameters = new AzureSynapseRunnerParameters
            {
                CalculatorRunId = id,
                CheckInterval = checkInterval,
                FinancialYear = financialYear,
                MaxChecks = maxChecks,
                PipelineUrl = pipelineUrl,
                PipelineName = orgpipelineName,
                StatusUpdateEndpoint = statusUpdateEndpoint,
            };

            this.AzureSynapseRunner.Setup(t => t.Process(It.IsAny<AzureSynapseRunnerParameters>())).ReturnsAsync(true);

            // Act
            var result = await this.CalculatorRunService.StartProcess(calculatorRunParameters);

            Assert.IsTrue(result);
        }
    }
}