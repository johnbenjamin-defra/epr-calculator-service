namespace EPR.Calculator.Service.Function.UnitTests
{
    using EPR.Calculator.Service.Function.Builder;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Builder.Detail;
    using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
    using EPR.Calculator.Service.Function.Builder.Lapcap;
    using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
    using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Builder.Summary;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private readonly Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder;
        private readonly Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private readonly Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private readonly Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder;
        private readonly Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private readonly Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;
        private readonly CalcResultBuilder calcResultBuilder;

        private readonly Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder;
        private readonly Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;
        private readonly Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder;

        public CalcResultBuilderTests()
        {
            this.mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            this.mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            this.mockCommsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            this.mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.mockCalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            this.mockOnePlusFourApportionmentBuilder = new Mock<ICalcResultOnePlusFourApportionmentBuilder>();
            this.mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            this.mockCalcResultScaledupProducersBuilder = new Mock<ICalcResultScaledupProducersBuilder>();

            this.calcResultBuilder = new CalcResultBuilder(
                this.mockCalcResultDetailBuilder.Object,
                this.mockLapcapBuilder.Object,
                this.mockCalcResultParameterOtherCostBuilder.Object,
                this.mockOnePlusFourApportionmentBuilder.Object,
                this.mockCommsCostReportBuilder.Object,
                this.mockLateReportingBuilder.Object,
                this.mockCalcRunLaDisposalCostBuilder.Object,
                this.mockCalcResultScaledupProducersBuilder.Object,
                this.mockSummaryBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                this.mockCalcResultDetailBuilder.Object,
                this.mockLapcapBuilder.Object,
                this.mockCalcResultParameterOtherCostBuilder.Object,
                this.mockOnePlusFourApportionmentBuilder.Object,
                this.mockCommsCostReportBuilder.Object,
                this.mockLateReportingBuilder.Object,
                this.mockCalcRunLaDisposalCostBuilder.Object,
                this.mockCalcResultScaledupProducersBuilder.Object,
                this.mockSummaryBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Build_ShouldReturnCalcResult()
        {
            var resultsRequestDto = new CalcResultsRequestDto();
            var mockResultDetail = new Mock<CalcResultDetail>();
            var mockLapcapData = new Mock<CalcResultLapcapData>();
            var mockOtherParams = new Mock<CalcResultParameterOtherCost>();
            var mockOnePlusFourApp = new Mock<CalcResultOnePlusFourApportionment>();
            var mockCalcResultCommsCost = new Mock<CalcResultCommsCost>();
            var mockCalcResultLateReportingTonnage = new Mock<CalcResultLateReportingTonnage>();
            var mockCalcResultLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
            var mockCalcResultSummary = new Mock<CalcResultSummary>();

            this.mockCalcResultDetailBuilder.Setup(m => m.Construct(resultsRequestDto)).ReturnsAsync(mockResultDetail.Object);
            this.mockLapcapBuilder.Setup(m => m.Construct(resultsRequestDto)).ReturnsAsync(mockLapcapData.Object);
            this.mockCalcResultParameterOtherCostBuilder.Setup(m => m.Construct(resultsRequestDto))
                .ReturnsAsync(mockOtherParams.Object);
            this.mockOnePlusFourApportionmentBuilder.Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
                .Returns(mockOnePlusFourApp.Object);
            this.mockCommsCostReportBuilder
                .Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultCommsCost.Object);
            this.mockLateReportingBuilder.Setup(m => m.Construct(resultsRequestDto))
                .ReturnsAsync(mockCalcResultLateReportingTonnage.Object);
            this.mockCalcRunLaDisposalCostBuilder.Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
            this.mockSummaryBuilder.Setup(x => x.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
                .ReturnsAsync(mockCalcResultSummary.Object);

            var results = this.calcResultBuilder.Build(resultsRequestDto);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(mockResultDetail.Object, result.CalcResultDetail);
            Assert.AreEqual(mockLapcapData.Object, result.CalcResultLapcapData);
            Assert.AreEqual(mockOtherParams.Object, result.CalcResultParameterOtherCost);
            Assert.AreEqual(mockOnePlusFourApp.Object, result.CalcResultOnePlusFourApportionment);
            Assert.AreEqual(mockCalcResultCommsCost.Object, result.CalcResultCommsCostReportDetail);
            Assert.AreEqual(mockCalcResultLateReportingTonnage.Object, result.CalcResultLateReportingTonnageData);
            Assert.AreEqual(mockCalcResultLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
            Assert.AreEqual(mockCalcResultSummary.Object, result.CalcResultSummary);
        }
    }
}
