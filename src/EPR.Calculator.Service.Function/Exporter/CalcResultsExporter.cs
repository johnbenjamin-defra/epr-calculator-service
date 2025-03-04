﻿namespace EPR.Calculator.API.Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Exporter.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Exporter.OtherCosts;
    using EPR.Calculator.Service.Function.Exporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultsExporter(
        LateReportingExporter lateReportingExporter,
        ICalcResultDetailExporter resultDetailexporter,
        IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
        ICalcResultLaDisposalCostExporter laDisposalCostExporter,
        ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
        ILapcaptDetailExporter lapcaptDetailExporter,
        ICalcResultParameterOtherCostExporter parameterOtherCosts,
        ICommsCostExporter commsCostExporter)
        : ICalcResultsExporter<CalcResult>
    {
        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            resultDetailexporter.Export(results.CalcResultDetail, csvContent);
            if (results.CalcResultLapcapData != null)
            {
                lapcaptDetailExporter.Export(results.CalcResultLapcapData, csvContent);
            }

            csvContent.Append(lateReportingExporter.PrepareData(results.CalcResultLateReportingTonnageData));

            if (results.CalcResultParameterOtherCost != null)
            {
                parameterOtherCosts.Export(results.CalcResultParameterOtherCost, csvContent);
            }

            onePlusFourApportionmentExporter.Export(results.CalcResultOnePlusFourApportionment, csvContent);

            if (results.CalcResultCommsCostReportDetail != null)
            {
                commsCostExporter.Export(results.CalcResultCommsCostReportDetail, csvContent);
            }

            if (results.CalcResultLaDisposalCostData != null)
            {
                laDisposalCostExporter.Export(results.CalcResultLaDisposalCostData, csvContent);
            }

            if (results.CalcResultScaledupProducers != null)
            {
                calcResultScaledupProducersExporter.Export(results.CalcResultScaledupProducers, csvContent);
            }

            if (results.CalcResultSummary != null)
            {
                PrepareSummaryData(results.CalcResultSummary, csvContent);
            }

            return csvContent.ToString();
        }

        private static void PrepareOtherCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(otherCost.Name);

            var saOperatingCosts = otherCost.SaOperatingCost.OrderBy(x => x.OrderId);

            foreach (var saOperatingCost in saOperatingCosts)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.England));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Total));
                csvContent.AppendLine();
            }

            csvContent.AppendLine();

            var laDataPreps = otherCost.Details.OrderBy(x => x.OrderId);

            foreach (var laDataPrep in laDataPreps)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.England));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Total));
                csvContent.AppendLine();
            }

            csvContent.AppendLine();
            var schemeCost = otherCost.SchemeSetupCost;
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.Name));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.England));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.Wales));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.Scotland));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.NorthernIreland));
            csvContent.AppendLine(CsvSanitiser.SanitiseData(schemeCost.Total));

            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Key));
            csvContent.AppendLine(CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Value));

            csvContent.AppendLine();
            var materiality = otherCost.Materiality;
            foreach (var material in materiality)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(material.SevenMateriality));
                csvContent.Append(CsvSanitiser.SanitiseData(material.Amount));
                csvContent.Append(CsvSanitiser.SanitiseData(material.Percentage));
                csvContent.AppendLine();
            }
        }

        private static void PrepareSummaryData(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareSummaryDataHeader(resultSummary, csvContent);

            // Add data
            foreach (var producer in resultSummary.ProducerDisposalFees)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.IsProducerScaledup));

                AppendProducerDisposalFeesByMaterial(csvContent, producer);

                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFee, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotal, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotal, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotal, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotal, DecimalPlaces.Two, null, true));

                AppendProducerCommsFeesByMaterial(csvContent, producer);

                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFee, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalComms, DecimalPlaces.Two, null, true));

                // Section-(1) & (2a) values
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforLADisposalCostswoBadDebtprovision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor1, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforLADisposalCostswithBadDebtprovision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));

                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor2A, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));

                // Percentage of Producer Reported Tonnage vs All Producers
                csvContent.Append(CsvSanitiser.SanitiseData(producer.PercentageofProducerReportedTonnagevsAllProducers, DecimalPlaces.Eight, null, false, true));

                // 2b comms Total
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithoutBadDebtFor2bComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor2bComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));

                // 2c comms Total
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCEnglandTotalWithBadDebt, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCWalesTotalWithBadDebt, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCScotlandTotalWithBadDebt, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCNorthernIrelandTotalWithBadDebt, DecimalPlaces.Two, null, true));

                // Total bill 1 + 2a + 2b + 2c
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, DecimalPlaces.Eight, null, false, true));

                // Section 3 Exported row 101
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Total3SAOperatingCostwoBadDebtprovision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor3, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.Total3SAOperatingCostswithBadDebtprovision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision3, DecimalPlaces.Two, null, true));

                // LA data prep costs section 4
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4, DecimalPlaces.Two, null, true));

                // Section-5 SA setup costs
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithoutBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionSection5, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvisionSection5, DecimalPlaces.Two, null, true));

                // Section-TotalBill
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionForTotalProducerBill, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvisionTotalBill, DecimalPlaces.Two, null, true));

                csvContent.AppendLine();
            }
        }

        private static void AppendProducerDisposalFeesByMaterial(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            foreach (var disposalFee in producer.ProducerDisposalFeesByMaterial!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (disposalFee.Key.Code == MaterialCodes.Glass)
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

        private static void AppendProducerCommsFeesByMaterial(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            foreach (var disposalFee in producer.ProducerCommsFeesByMaterial!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (disposalFee.Key.Code == MaterialCodes.Glass)
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

        private static void PrepareSummaryDataHeader(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add result summary header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.ResultSummaryHeader?.Name))
                .AppendLine()
                .AppendLine();

            // Add notes header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.NotesHeader?.Name));

            // Add producer disposal fees header
            WriteSecondaryHeaders(csvContent, resultSummary.ProducerDisposalFeesHeaders);

            // Add material breakdown header
            WriteSecondaryHeaders(csvContent, resultSummary.MaterialBreakdownHeaders);

            // Add column header
            WriteColumnHeaders(resultSummary, csvContent);

            csvContent.AppendLine();
        }

        private static void WriteSecondaryHeaders(StringBuilder csvContent, IEnumerable<CalcResultSummaryHeader> headers)
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

        private static void WriteColumnHeaders(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            foreach (var item in resultSummary.ColumnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}