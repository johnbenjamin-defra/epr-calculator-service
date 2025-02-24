namespace EPR.Calculator.API.Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private const string RunName = "Run Name";
        private const string RunId = "Run Id";
        private const string RunDate = "Run Date";
        private const string Runby = "Run by";
        private const string FinancialYear = "Financial Year";
        private const string RPDFileORG = "RPD File - ORG";
        private const string RPDFilePOM = "RPD File - POM";
        private const string LapcapFile = "LAPCAP File";
        private const string ParametersFile = "Parameters File";
        private const string CountryApportionmentFile = "Country Apportionment File";
        private const string DecimalFormat = "F3";
        private const int DecimalRoundUp = 2;
        private const int DecimalPoint = 3;

        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            LoadCalcResultDetail(results, csvContent);
            if (results.CalcResultLapcapData != null)
            {
                PrepareLapcapData(results.CalcResultLapcapData, csvContent);
            }

            if (results.CalcResultLateReportingTonnageData != null)
            {
                PrepareLateReportingData(results.CalcResultLateReportingTonnageData, csvContent);
            }

            if (results.CalcResultParameterOtherCost != null)
            {
                PrepareOtherCosts(results.CalcResultParameterOtherCost, csvContent);
            }

            if (results.CalcResultOnePlusFourApportionment != null)
            {
                PrepareOnePluseFourApportionment(results.CalcResultOnePlusFourApportionment, csvContent);
            }

            if (results.CalcResultCommsCostReportDetail != null)
            {
                PrepareCommsCost(results.CalcResultCommsCostReportDetail, csvContent);
            }

            if (results.CalcResultLaDisposalCostData != null)
            {
                PrepareLaDisposalCostData(results.CalcResultLaDisposalCostData, csvContent);
            }

            if (results.CalcResultScaledupProducers != null)
            {
                PrepareScaledupProducers(results.CalcResultScaledupProducers, csvContent);
            }

            if (results.CalcResultSummary != null)
            {
                PrepareSummaryData(results.CalcResultSummary, csvContent);
            }

            return csvContent.ToString();
        }

        private static void PrepareCommsCost(CalcResultCommsCost communicationCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(communicationCost.Name);

            var apportionment = communicationCost.CalcResultCommsCostOnePlusFourApportionment;

            foreach (var onePlusFourApportionment in apportionment)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Total)}");
            }

            csvContent.AppendLine();
            var commCostByMaterials = communicationCost.CalcResultCommsCostCommsCostByMaterial;

            foreach (var commCostByMaterial in commCostByMaterials)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(commCostByMaterial.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commCostByMaterial.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commCostByMaterial.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commCostByMaterial.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commCostByMaterial.NorthernIreland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commCostByMaterial.Total)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnage)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commCostByMaterial.ReportedPublicBinTonnage)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commCostByMaterial.HouseholdDrinksContainers)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commCostByMaterial.LateReportingTonnage)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPlusLateReportingTonnage)},");
                csvContent.AppendLine(commCostByMaterial.Total == "Total" || string.IsNullOrWhiteSpace(commCostByMaterial.CommsCostByMaterialPricePerTonne) ?
                      $"{CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne)}" :
                     $"£{CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne)}");
            }

            csvContent.AppendLine();
            var countryList = communicationCost.CommsCostByCountry;
            foreach (var country in countryList)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(country.Total)}");
            }
        }

        private static void PrepareOtherCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(otherCost.Name);

            var saOperatingCosts = otherCost.SaOperatingCost.OrderBy(x => x.OrderId);

            foreach (var saOperatingCost in saOperatingCosts)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(saOperatingCost.Total)}");
            }

            csvContent.AppendLine();

            var laDataPreps = otherCost.Details.OrderBy(x => x.OrderId);

            foreach (var laDataPrep in laDataPreps)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(laDataPrep.Total)}");
            }

            csvContent.AppendLine();
            var schemeCost = otherCost.SchemeSetupCost;
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Name)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.England)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Wales)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Scotland)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.NorthernIreland)},");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(schemeCost.Total)}");

            csvContent.AppendLine();
            csvContent.Append($"{CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Key)},");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Value)}");

            csvContent.AppendLine();
            var materiality = otherCost.Materiality;
            foreach (var material in materiality)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.SevenMateriality)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.Amount)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(material.Percentage)}");
            }
        }

        private static void LoadCalcResultDetail(CalcResult results, StringBuilder csvContent)
        {
            AppendCsvLine(csvContent, RunName, results.CalcResultDetail.RunName);
            AppendCsvLine(csvContent, RunId, results.CalcResultDetail.RunId.ToString());
            AppendCsvLine(csvContent, RunDate, results.CalcResultDetail.RunDate.ToString(CalculationResults.DateFormat));
            AppendCsvLine(csvContent, Runby, results.CalcResultDetail.RunBy);
            AppendCsvLine(csvContent, FinancialYear, results.CalcResultDetail.FinancialYear);
            AppendRpdFileInfo(csvContent, RPDFileORG, RPDFilePOM, results.CalcResultDetail.RpdFileORG, results.CalcResultDetail.RpdFilePOM);
            AppendFileInfo(csvContent, LapcapFile, results.CalcResultDetail.LapcapFile);
            AppendFileInfo(csvContent, ParametersFile, results.CalcResultDetail.ParametersFile);
            AppendFileInfo(csvContent, CountryApportionmentFile, results.CalcResultDetail.CountryApportionmentFile);
        }

        private static void AppendRpdFileInfo(StringBuilder csvContent, string rPDFileORG, string rPDFilePOM, string rpdFileORGValue, string rpdFilePOMValue)
        {
            csvContent.AppendLine($"{rPDFileORG},{CsvSanitiser.SanitiseData(rpdFileORGValue)},{rPDFilePOM},{CsvSanitiser.SanitiseData(rpdFilePOMValue)}");
        }

        public static void AppendFileInfo(StringBuilder csvContent, string label, string filePath)
        {
            var fileParts = filePath.Split(',');
            if (fileParts.Length >= 3)
            {
                string fileName = CsvSanitiser.SanitiseData(fileParts[0]);
                string date = CsvSanitiser.SanitiseData(fileParts[1]);
                string user = CsvSanitiser.SanitiseData(fileParts[2]);
                csvContent.AppendLine($"{label},{fileName},{date},{user}");
            }
        }

        private static void AppendCsvLine(StringBuilder csvContent, string label, string value)
        {
            csvContent.AppendLine($"{label},{CsvSanitiser.SanitiseData(value)}");
        }

        private static void PrepareLapcapData(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLapcapData.Name);
            var lapcapDataDetails = calcResultLapcapData.CalcResultLapcapDataDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.TotalDisposalCost)}\"");
                csvContent.AppendLine();
            }
        }

        private static void PrepareOnePluseFourApportionment(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResult1Plus4Apportionment.Name);
            var lapcapDataDetails = calcResult1Plus4Apportionment.CalcResultOnePlusFourApportionmentDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\",");
                csvContent.AppendLine();
            }
        }

        private static void PrepareLateReportingData(CalcResultLateReportingTonnage calcResultLateReportingData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLateReportingData.Name);
            csvContent.Append($"{calcResultLateReportingData.MaterialHeading},");
            csvContent.Append(calcResultLateReportingData.TonnageHeading);
            csvContent.AppendLine();

            foreach (var lateReportingData in calcResultLateReportingData.CalcResultLateReportingTonnageDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lateReportingData.Name)},");
                csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.TotalLateReportingTonnage));
                csvContent.AppendLine();
            }
        }

        private static void PrepareLaDisposalCostData(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLaDisposalCostData.Name);
            var lapcapDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.England)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Wales)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Scotland)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIreland)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdPackagingWasteTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ReportedPublicBinTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.HouseholdDrinkContainers)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.LateReportingTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ProducerReportedTotalTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.DisposalCostPricePerTonne)}\",");
                csvContent.AppendLine();
            }
        }

        private static void PrepareScaledupProducers(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareScaledupProducersHeader(producers, csvContent);

            // Add data
            if (producers.ScaledupProducers != null)
            {
                foreach (var producer in producers.ScaledupProducers)
                {
                    if (producer.IsTotalRow)
                    {
                        _ = csvContent.Append(new string(CommonConstants.Comma[0], 7));
                        csvContent.Append($"{CommonConstants.Totals},");
                    }
                    else
                    {
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ProducerId)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.SubsidiaryId)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ProducerName)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.Level)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.DaysInSubmissionPeriod != -1 ? producer.DaysInSubmissionPeriod.ToString() : string.Empty)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.DaysInWholePeriod != -1 ? producer.DaysInWholePeriod.ToString() : string.Empty)},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ScaleupFactor == -1 ? CommonConstants.Totals : producer.ScaleupFactor.ToString())},");
                    }

                    foreach (var producerTonnage in producer.ScaledupProducerTonnageByMaterial)
                    {
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ReportedHouseholdPackagingWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ReportedPublicBinTonnage, DecimalPoint).ToString(DecimalFormat))},");

                        if (producerTonnage.Key == MaterialCodes.Glass || producerTonnage.Key == MaterialNames.Glass)
                        {
                            csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.HouseholdDrinksContainersTonnageGlass, DecimalPoint).ToString(DecimalFormat))},");
                        }

                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.TotalReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ReportedSelfManagedConsumerWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.NetReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ScaledupReportedHouseholdPackagingWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ScaledupReportedPublicBinTonnage, DecimalPoint).ToString(DecimalFormat))},");

                        if (producerTonnage.Key == MaterialCodes.Glass || producerTonnage.Key == MaterialNames.Glass)
                        {
                            csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ScaledupHouseholdDrinksContainersTonnageGlass, DecimalPoint).ToString(DecimalFormat))},");
                        }

                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ScaledupTotalReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ScaledupReportedSelfManagedConsumerWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producerTonnage.Value.ScaledupNetReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    }

                    csvContent.AppendLine();
                }
            }
            else
            {
                csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.NoScaledupProducers));
            }
        }

        private static void PrepareScaledupProducersHeader(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            // Add scaledup producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(producers.TitleHeader.Name));
            csvContent.AppendLine();

            // Add material breakdown header
            WriteScaledupProducersSecondaryHeaders(producers.MaterialBreakdownHeaders, csvContent);

            // Add column header
            WriteScaledupProducersColumnHeaders(producers, csvContent);
            csvContent.AppendLine();
        }

        private static void WriteScaledupProducersSecondaryHeaders(IEnumerable<CalcResultScaledupProducerHeader> headers, StringBuilder csvContent)
        {
            const int maxColumnSize = CommonConstants.SecondaryHeaderMaxColumnSize;
            var headerRows = new string[maxColumnSize];
            foreach (var item in headers)
            {
                if (item.ColumnIndex.HasValue)
                {
                    headerRows[item.ColumnIndex.Value - 1] = $"{CsvSanitiser.SanitiseData(item.Name)}";
                }
            }

            var headerRow = string.Join(CommonConstants.Comma, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WriteScaledupProducersColumnHeaders(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            foreach (var item in producers.ColumnHeaders)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(item.Name)},");
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
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ProducerId)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.SubsidiaryId)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ProducerName)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.Level)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.isProducerScaledup)},");

                foreach (var disposalFee in producer.ProducerDisposalFeesByMaterial)
                {
                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");

                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.PublicBinTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    if (disposalFee.Key.Code == MaterialCodes.Glass)
                    {
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.HouseholdDrinksContainersTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    }

                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.TotalReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ManagedConsumerWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.NetReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    csvContent.Append(producer.isProducerScaledup != CommonConstants.Totals ? $"£{CsvSanitiser.SanitiseData(disposalFee.Value.PricePerTonne)}," : ",");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ProducerDisposalFee, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.BadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ProducerDisposalFeeWithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.EnglandWithBadDebtProvision, DecimalRoundUp).ToString("F2"))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.WalesWithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalRoundUp))},");
                }

                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerDisposalFee, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerDisposalFeeWithBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotal, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotal, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotal, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotal, DecimalRoundUp))},");

                foreach (var disposalFee in producer.ProducerCommsFeesByMaterial)
                {
                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ReportedPublicBinTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    if (disposalFee.Key.Code == MaterialCodes.Glass)
                    {
                        csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.HouseholdDrinksContainers, DecimalPoint).ToString(DecimalFormat))},");
                    }

                    csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.TotalReportedTonnage, DecimalPoint).ToString(DecimalFormat))},");
                    csvContent.Append(producer.isProducerScaledup != CommonConstants.Totals ? $"£{CsvSanitiser.SanitiseData(disposalFee.Value.PriceperTonne)}," : ",");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ProducerTotalCostWithoutBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.BadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ProducerTotalCostwithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.EnglandWithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.WalesWithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalRoundUp))},");
                    csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalRoundUp))},");
                }

                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerCommsFee, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerCommsFeeWithBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalComms, DecimalRoundUp))},");

                // Section-(1) & (2a) values
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeforLADisposalCostswoBadDebtprovision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionFor1, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeforLADisposalCostswithBadDebtprovision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalWithBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalWithBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalWithBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalWithBadDebtProvision, DecimalRoundUp))},");

                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionFor2A, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalWithBadDebtProvision2A, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalWithBadDebtProvision2A, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalWithBadDebtProvision2A, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalWithBadDebtProvision2A, DecimalRoundUp))},");

                // Percentage of Producer Tonnage vs All Producers
                csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producer.PercentageofProducerReportedTonnagevsAllProducers, 8))}%,");

                // 2b comms Total
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeWithoutBadDebtFor2bComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionFor2bComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeWithBadDebtFor2bComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalWithBadDebtFor2bComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalWithBadDebtFor2bComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalWithBadDebtFor2bComms, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalWithBadDebtFor2bComms, DecimalRoundUp))},");

                // 2c comms Total
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCEnglandTotalWithBadDebt, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCWalesTotalWithBadDebt, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCScotlandTotalWithBadDebt, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TwoCNorthernIrelandTotalWithBadDebt, DecimalRoundUp))},");

                // Total bill 1 + 2a + 2b + 2c
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalRoundUp))},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(Math.Round(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, 8))}%,");

                // Section 3 Exported row 101
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.Total3SAOperatingCostwoBadDebtprovision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionFor3, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.Total3SAOperatingCostswithBadDebtprovision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalWithBadDebtProvision3, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalWithBadDebtProvision3, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalWithBadDebtProvision3, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalWithBadDebtProvision3, DecimalRoundUp))},");

                // LA data prep costs section 4
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsTotalWithoutBadDebtProvisionSection4, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsBadDebtProvisionSection4, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsTotalWithBadDebtProvisionSection4, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsEnglandTotalWithBadDebtProvisionSection4, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsWalesTotalWithBadDebtProvisionSection4, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsScotlandTotalWithBadDebtProvisionSection4, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.LaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionSection4, DecimalRoundUp))},");

                // Section-5 SA setup costs
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeWithoutBadDebtProvisionSection5, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionSection5, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerFeeWithBadDebtProvisionSection5, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalWithBadDebtProvisionSection5, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalWithBadDebtProvisionSection5, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalWithBadDebtProvisionSection5, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalWithBadDebtProvisionSection5, DecimalRoundUp))},");

                // Section-TotalBill
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerBillWithoutBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.BadDebtProvisionForTotalProducerBill, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.TotalProducerBillWithBadDebtProvision, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.EnglandTotalWithBadDebtProvisionTotalBill, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.WalesTotalWithBadDebtProvisionTotalBill, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.ScotlandTotalWithBadDebtProvisionTotalBill, DecimalRoundUp))},");
                csvContent.Append($"£{CsvSanitiser.SanitiseData(Math.Round(producer.NorthernIrelandTotalWithBadDebtProvisionTotalBill, DecimalRoundUp))},");

                csvContent.AppendLine();
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
                    headerRows[item.ColumnIndex.Value - 1] = $"{CsvSanitiser.SanitiseData(item.Name)}";
                }
            }

            var headerRow = string.Join(CommonConstants.Comma, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WriteColumnHeaders(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            foreach (var item in resultSummary.ColumnHeaders)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(item.Name)},");
            }
        }
    }
}