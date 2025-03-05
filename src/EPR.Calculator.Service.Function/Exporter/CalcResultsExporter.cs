namespace EPR.Calculator.API.Exporter
{
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Exporter.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.Detail;
    using EPR.Calculator.Service.Function.Exporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Exporter.OtherCosts;
    using EPR.Calculator.Service.Function.Exporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;
    using System;
    using System.Linq;
    using System.Text;

    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporter;
        private readonly ICalcResultDetailExporter resultDetailexporter;
        private readonly IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter;
        private readonly ILapcaptDetailExporter lapcaptDetailExporter;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCosts;
        private readonly ILateReportingExporter lateReportingExporter;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporter;
        private readonly ICommsCostExporter commsCostExporter;

        public CalcResultsExporter(
            ILateReportingExporter lateReportingExporter,
            ICalcResultDetailExporter resultDetailexporter,
            IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostExporter laDisposalCostExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
            ILapcaptDetailExporter lapcaptDetailExporter,
            ICalcResultParameterOtherCostExporter parameterOtherCosts,
            ICommsCostExporter commsCostExporter,
            ICalcResultSummaryExporter calcResultSummaryExporter)
        {
            this.resultDetailexporter = resultDetailexporter;
            this.onePlusFourApportionmentExporter = onePlusFourApportionmentExporter;
            this.lateReportingExporter = lateReportingExporter;
            this.calcResultScaledupProducersExporter = calcResultScaledupProducersExporter;
            this.lapcaptDetailExporter = lapcaptDetailExporter;
            this.parameterOtherCosts = parameterOtherCosts;
            this.calcResultSummaryExporter = calcResultSummaryExporter;
            this.laDisposalCostExporter = laDisposalCostExporter;
            this.commsCostExporter = commsCostExporter;
        }

        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            this.resultDetailexporter.Export(results.CalcResultDetail, csvContent);

            this.lapcaptDetailExporter.Export(results.CalcResultLapcapData, csvContent);

            csvContent.Append(lateReportingExporter.Export(results.CalcResultLateReportingTonnageData));

            this.parameterOtherCosts.Export(results.CalcResultParameterOtherCost, csvContent);

            this.onePlusFourApportionmentExporter.Export(results.CalcResultOnePlusFourApportionment, csvContent);

            this.commsCostExporter.Export(results.CalcResultCommsCostReportDetail, csvContent);

            this.laDisposalCostExporter.Export(results.CalcResultLaDisposalCostData, csvContent);

            this.calcResultScaledupProducersExporter.Export(results.CalcResultScaledupProducers, csvContent);

            this.calcResultSummaryExporter.Export(results.CalcResultSummary, csvContent);

            return csvContent.ToString();
        }
    }
}