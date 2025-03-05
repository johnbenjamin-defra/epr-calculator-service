using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter
{
    public class CalcResultSummaryExporter : ICalcResultSummaryExporter
    {
        private readonly IEnumerable<string> extraColumns = [MaterialCodes.Glass];

        public void Export(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            this.PrepareSummaryDataHeader(resultSummary, csvContent);

            // Add data
            foreach (var producer in resultSummary.ProducerDisposalFees)
            {
                AddNewRow(csvContent, producer);
            }
        }

        public void AddNewRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            this.AddFirstFiveColumns(csvContent, producer);

            this.AppendProducerDisposalFeesByMaterial(csvContent, producer);

            this.AddProducerDisposal(csvContent, producer);

            this.AppendProducerCommsFeesByMaterial(csvContent, producer);

            this.AddProducerCommsFee(csvContent, producer);

            this.AddProducerFee(csvContent, producer);

            this.AddProducerFeeForComms(csvContent, producer);

            csvContent.Append(CsvSanitiser.SanitiseData(producer.PercentageofProducerReportedTonnagevsAllProducers, DecimalPlaces.Eight, null, false, true));

            this.AddTotalProducerFee(csvContent, producer);

            // 2c comms Total
            this.AddTwoC(csvContent, producer);

            // Total bill 1 + 2a + 2b + 2c
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, DecimalPlaces.Eight, null, false, true));

            this.AddTotal3SA(csvContent, producer);

            // LA data prep costs section 4
            this.AddLaDataPrepCosts(csvContent, producer);

            this.AddSection5(csvContent, producer);
            this.AddTotalSection(csvContent, producer);

            csvContent.AppendLine();
        }

        public void AddTotalSection(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionForTotalProducerBill, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
        }

        public void AddSection5(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithoutBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionSection5, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
        }

        public void AddLaDataPrepCosts(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
        }

        public void AddTotal3SA(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.Total3SAOperatingCostwoBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor3, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.Total3SAOperatingCostswithBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
        }

        public void AddTwoC(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCEnglandTotalWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCWalesTotalWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCScotlandTotalWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCNorthernIrelandTotalWithBadDebt, DecimalPlaces.Two, null, true));
        }

        public void AddTotalProducerFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithoutBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
        }

        public void AddProducerFeeForComms(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
        }

        public void AddProducerCommsFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFee, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalComms, DecimalPlaces.Two, null, true));
        }

        public void AddProducerFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforLADisposalCostswoBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor1, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforLADisposalCostswithBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        }

        public void AddProducerDisposal(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFee, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotal, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotal, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotal, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotal, DecimalPlaces.Two, null, true));
        }

        public void AddFirstFiveColumns(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.IsProducerScaledup));
        }

        private void AppendProducerDisposalFeesByMaterial(
            StringBuilder csvContent,
            CalcResultSummaryProducerDisposalFees producer)
        {
            foreach (var disposalFee in producer.ProducerDisposalFeesByMaterial!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (this.extraColumns.Contains(disposalFee.Key.Code))
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(producer.IsProducerScaledup != CommonConstants.Totals ? CsvSanitiser.SanitiseData(disposalFee.Value.PricePerTonne, null, null, true) : CommonConstants.CsvFileDelimiter);
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerDisposalFee, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalPlaces.Two, null, true));
            }
        }

        private void AppendProducerCommsFeesByMaterial(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            foreach (var disposalFee in producer.ProducerCommsFeesByMaterial!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (this.extraColumns.Contains(disposalFee.Key.Code))
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainers, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(producer.IsProducerScaledup != CommonConstants.Totals ? CsvSanitiser.SanitiseData(disposalFee.Value.PriceperTonne, null, null, true) : CommonConstants.CsvFileDelimiter);
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostwithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalPlaces.Two, null, true));
            }
        }

        public void WriteSecondaryHeaders(StringBuilder csvContent, IEnumerable<CalcResultSummaryHeader> headers)
        {
            const int maxColumnSize = CommonConstants.SecondaryHeaderMaxColumnSize;
            var headerRows = new string[maxColumnSize];
            foreach (var item in headers)
            {
                if (item.ColumnIndex.HasValue)
                {
                    headerRows[item.ColumnIndex.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
                }
            }

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        public void WriteColumnHeaders(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            foreach (var item in resultSummary.ColumnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }

        private void PrepareSummaryDataHeader(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add result summary header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.ResultSummaryHeader?.Name))
                .AppendLine()
                .AppendLine();

            // Add notes header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.NotesHeader?.Name));

            // Add producer disposal fees header
            this.WriteSecondaryHeaders(csvContent, resultSummary.ProducerDisposalFeesHeaders);

            // Add material breakdown header
            this.WriteSecondaryHeaders(csvContent, resultSummary.MaterialBreakdownHeaders);

            // Add column header
            this.WriteColumnHeaders(resultSummary, csvContent);

            csvContent.AppendLine();
        }
    }
}
