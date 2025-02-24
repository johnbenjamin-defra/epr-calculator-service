namespace EPR.Calculator.Service.Function.Builder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
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

    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder;
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultSummaryBuilder summaryBuilder;
        private readonly ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder;
        private readonly ICalcResultCommsCostBuilder commsCostReportBuilder;
        private readonly ICalcResultLateReportingBuilder lateReportingBuilder;
        private readonly ICalcRunLaDisposalCostBuilder laDisposalCostBuilder;
        private readonly ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultBuilder"/> class.
        /// </summary>
        public CalcResultBuilder(
            ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
            ICalcResultOnePlusFourApportionmentBuilder calcResultOnePlusFourApportionmentBuilder,
            ICalcResultCommsCostBuilder commsCostReportBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder,
            ICalcRunLaDisposalCostBuilder calcRunLaDisposalCostBuilder,
            ICalcResultScaledupProducersBuilder calcResultScaledupProducersBuilder,
            ICalcResultSummaryBuilder summaryBuilder)
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.commsCostReportBuilder = commsCostReportBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
            this.calcResultParameterOtherCostBuilder = calcResultParameterOtherCostBuilder;
            this.laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
            this.lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionmentBuilder;
            this.calcResultScaledupProducersBuilder = calcResultScaledupProducersBuilder;
            this.summaryBuilder = summaryBuilder;
        }

        public async Task<CalcResult> Build(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResult
            {
                CalcResultLapcapData =
                new CalcResultLapcapData
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>(),
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    Name = string.Empty,
                },
            };
            result.CalcResultScaledupProducers = await this.calcResultScaledupProducersBuilder.Construct(resultsRequestDto);
            result.CalcResultDetail = await this.calcResultDetailBuilder.Construct(resultsRequestDto);
            result.CalcResultLapcapData = await this.lapcapBuilder.Construct(resultsRequestDto);
            result.CalcResultLateReportingTonnageData = await this.lateReportingBuilder.Construct(resultsRequestDto);
            result.CalcResultParameterOtherCost = await this.calcResultParameterOtherCostBuilder.Construct(resultsRequestDto);
            result.CalcResultOnePlusFourApportionment = this.lapcapplusFourApportionmentBuilder.Construct(resultsRequestDto, result);
            result.CalcResultScaledupProducers = await this.calcResultScaledupProducersBuilder.Construct(resultsRequestDto);
            result.CalcResultCommsCostReportDetail = await this.commsCostReportBuilder.Construct(
                resultsRequestDto, result.CalcResultOnePlusFourApportionment, result);
            result.CalcResultLaDisposalCostData = await this.laDisposalCostBuilder.Construct(resultsRequestDto, result);
            
            result.CalcResultSummary = await this.summaryBuilder.Construct(resultsRequestDto, result);

            return result;
        }
    }
}