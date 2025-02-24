namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
    using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
    using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.ThreeSA;
    using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
    using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    public static class CalcResultSummaryUtil
    {
        public const int ResultSummaryHeaderColumnIndex = 1;
        public const int NotesHeaderColumnIndex = 1;
        public const int ProducerDisposalFeesHeaderColumnIndex = 6;
        public const int CommsCostHeaderColumnIndex = 118;
        public const int MaterialsBreakdownHeaderInitialColumnIndex = 6;
        public const int MaterialsBreakdownHeaderIncrementalColumnIndex = 13;

        public const int DisposalFeeSummaryColumnIndex = 111;
        public const int MaterialsBreakdownHeaderCommsInitialColumnIndex = 118;
        public const int MaterialsBreakdownHeaderCommsIncrementalColumnIndex = 11;

        // Section-(1) & (2a)
        public const int DisposalFeeCommsCostsHeaderInitialColumnIndex = 214;

        // Section-(2b)
        private const int CommsCost2bColumnIndex = 229;
        public const int decimalRoundUp = 2;

        public static int GetLevelIndex(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer)
        {
            var totalRow = producerDisposalFeesLookup.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString() && pdf.isTotalRow);

            return totalRow == null ? (int)CalcResultSummaryLevelIndex.One : (int)CalcResultSummaryLevelIndex.Two;
        }

        public static bool IsProducerScaledup(
            ProducerDetail producer,
            IEnumerable<CalcResultScaledupProducer> scaledupProducers)
        {
            var scaledupProducer = scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId);
            return scaledupProducer != null;
        }

        public static decimal GetTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            string packagingType,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var scaledupProducerForAllSubmissionPeriods = scaledUpProducers.Where(p => p.ProducerId == producer.ProducerId
                && p.SubsidiaryId == producer.SubsidiaryId
                && !p.IsSubtotalRow
                && !p.IsTotalRow);

            if (scaledupProducerForAllSubmissionPeriods.Any())
            {
                decimal tonnage = 0;
                foreach (var item in scaledupProducerForAllSubmissionPeriods)
                {
                    switch (packagingType)
                    {
                        case PackagingTypes.Household:
                            tonnage += item.ScaledupProducerTonnageByMaterial[material.Code].ScaledupReportedHouseholdPackagingWasteTonnage;
                            break;
                        case PackagingTypes.PublicBin:
                            tonnage += item.ScaledupProducerTonnageByMaterial[material.Code].ScaledupReportedPublicBinTonnage;
                            break;
                        case PackagingTypes.ConsumerWaste:
                            tonnage += item.ScaledupProducerTonnageByMaterial[material.Code].ScaledupReportedSelfManagedConsumerWasteTonnage;
                            break;
                        case PackagingTypes.HouseholdDrinksContainers:
                            tonnage += item.ScaledupProducerTonnageByMaterial[material.Code].ScaledupHouseholdDrinksContainersTonnageGlass;
                            break;
                        default:
                            tonnage += 0;
                            break;
                    }
                }

                return tonnage;
            }

            var reportedMaterials = producer.ProducerReportedMaterials
                .FirstOrDefault(p => p.Material?.Code == material.Code && p.PackagingType == packagingType);

            return reportedMaterials?.PackagingTonnage ?? 0;
        }

        public static decimal GetTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            string packagingType,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetTonnage(producer, material, packagingType, scaledUpProducers));
        }

        public static decimal GetReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var householdPackagingWasteTonnage = GetTonnage(producer, material, PackagingTypes.Household, scaledUpProducers);
            var publicBinTonnage = GetTonnage(producer, material, PackagingTypes.PublicBin, scaledUpProducers);

            if (material.Code != MaterialCodes.Glass)
            {
                return householdPackagingWasteTonnage + publicBinTonnage;
            }

            var householdDrinksContainersTonnage = GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledUpProducers);

            return householdPackagingWasteTonnage + publicBinTonnage + householdDrinksContainersTonnage;
        }

        public static decimal GetReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers));
        }

        public static decimal GetNetReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var reportedTonnage = GetReportedTonnage(producer, material, scaledUpProducers);
            var managedConsumerWasteTonnage = GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers);

            return reportedTonnage - managedConsumerWasteTonnage;
        }

        public static decimal GetNetReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetNetReportedTonnage(producer, material, scaledUpProducers));
        }

        public static decimal GetPricePerTonne(
            MaterialDetail material,
            CalcResult calcResult)
        {
            var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.FirstOrDefault(la => la.Name == material.Name);

            if (laDisposalCostDataDetail == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(laDisposalCostDataDetail.DisposalCostPricePerTonne, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out decimal value);

            return isParseSuccessful ? value : 0;
        }

        public static decimal GetProducerDisposalFee(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var netReportedTonnage = GetNetReportedTonnage(producer, material, scaledUpProducers);
            var pricePerTonne = GetPricePerTonne(material, calcResult);

            return netReportedTonnage * pricePerTonne;
        }

        public static decimal GetProducerDisposalFeeProducerTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetProducerDisposalFee(producer, material, calcResult, scaledUpProducers));
        }

        public static decimal GetBadDebtProvision(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult, scaledUpProducers);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return producerDisposalFee * value / 100;
            }

            return 0;
        }

        public static decimal GetBadDebtProvisionProducerTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetBadDebtProvision(producer, material, calcResult, scaledUpProducers));
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvision(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult, scaledUpProducers);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return producerDisposalFee * (1 + (value / 100));
            }

            return 0;
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult, scaledUpProducers));
        }

        public static decimal GetCountryBadDebtProvision(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult,
            Countries country,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult, scaledUpProducers);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            string? disposalCost;
            switch (country)
            {
                case Countries.England:
                    disposalCost = countryApportionmentPercentage.EnglandDisposalCost;
                    break;
                case Countries.Wales:
                    disposalCost = countryApportionmentPercentage.WalesDisposalCost;
                    break;
                case Countries.Scotland:
                    disposalCost = countryApportionmentPercentage.ScotlandDisposalCost;
                    break;
                case Countries.NorthernIreland:
                    disposalCost = countryApportionmentPercentage.NorthernIrelandDisposalCost;
                    break;
                default:
                    disposalCost = "0%";
                    break;
            }

            var isParseSuccessful = decimal.TryParse(disposalCost.Replace("%", string.Empty), out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value / 100 : 0;
        }

        public static decimal GetCountryBadDebtProvisionTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            Countries country,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            return producers.Sum(producer => GetCountryBadDebtProvision(producer, material, calcResult, country, scaledUpProducers));
        }

        public static CalcResultLapcapDataDetails? GetCountryApportionmentPercentage(CalcResult calcResult)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == CalcResultSummaryHeaders.OneCountryApportionment);
        }

        public static decimal GetTotalProducerDisposalFee(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalProducerDisposalFee = 0;

            foreach (var material in materialCostSummary)
            {
                totalProducerDisposalFee += material.Value.ProducerDisposalFee;
            }

            return totalProducerDisposalFee;
        }

        public static decimal GetTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalBadDebtProvision += material.Value.BadDebtProvision;
            }

            return totalBadDebtProvision;
        }

        public static decimal GetTotalProducerDisposalFeeWithBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalProducerDisposalFeeWithBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalProducerDisposalFeeWithBadDebtProvision += material.Value.ProducerDisposalFeeWithBadDebtProvision;
            }

            return totalProducerDisposalFeeWithBadDebtProvision;
        }

        public static decimal GetEnglandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalEngland = 0;

            foreach (var material in materialCostSummary)
            {
                totalEngland += material.Value.EnglandWithBadDebtProvision;
            }

            return totalEngland;
        }

        public static decimal GetWalesTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalWales = 0;

            foreach (var material in materialCostSummary)
            {
                totalWales += material.Value.WalesWithBadDebtProvision;
            }

            return totalWales;
        }

        public static decimal GetScotlandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalScotland = 0;

            foreach (var material in materialCostSummary)
            {
                totalScotland += material.Value.ScotlandWithBadDebtProvision;
            }

            return totalScotland;
        }

        public static decimal GetNorthernIrelandTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            decimal totalNorthernIreland = 0;

            foreach (var material in materialCostSummary)
            {
                totalNorthernIreland += material.Value.NorthernIrelandWithBadDebtProvision;
            }

            return totalNorthernIreland;
        }

        public static decimal GetTotal1Plus2ABadDebt(
            IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            decimal total = 0m;

            foreach (var material in materials)
            {
                var laDisposalTotal = GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producers, material, calcResult, scaledUpProducers);
                var twoAcommsDisposal = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producers, material, calcResult, scaledUpProducers);
                total += laDisposalTotal + twoAcommsDisposal;
            }

            return total;
        }

        public static void SetHeaders(CalcResultSummary result, IEnumerable<MaterialDetail> materials)
        {
            result.ResultSummaryHeader = new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CalculationResult,
                ColumnIndex = ResultSummaryHeaderColumnIndex,
            };

            result.NotesHeader = new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.Notes,
                ColumnIndex = ResultSummaryHeaderColumnIndex,
            };

            result.ProducerDisposalFeesHeaders = GetProducerDisposalFeesHeaders();

            result.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(result, materials);

            result.ColumnHeaders = GetColumnHeaders(materials);
        }

        public static IEnumerable<CalcResultSummaryHeader> GetProducerDisposalFeesHeaders()
        {
            var resultSummaryHeaders = new List<CalcResultSummaryHeader>();

            resultSummaryHeaders.AddRange([
                // Section-1 Title headers
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision, ColumnIndex = ProducerDisposalFeesHeaderColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeader, ColumnIndex = CommsCostHeaderColumnIndex },

                // Section-(1) & (2a) Title headers
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswoBadDebtprovision1, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision,ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswithBadDebtprovision1, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 7 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 8 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwithBadDebtprovision2A,ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 9 },

                // Section-2b Title headers
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderWithoutBadDebtFor2bTitle, ColumnIndex = CommsCost2bColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderBadDebtProvisionFor2bTitle, ColumnIndex = CommsCost2bColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderWithBadDebtFor2bTitle, ColumnIndex = CommsCost2bColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithout, ColumnIndex = TwoCCommsCostColumnIndex.Value },
                new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostBadBebtProvision, ColumnIndex = TwoCCommsCostColumnIndex.Value + 1 },
                new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithBadDebt, ColumnIndex = TwoCCommsCostColumnIndex.Value + 2 },
            ]);

            // Section Total bill (1 + 2a + 2b + 2c)
            resultSummaryHeaders.AddRange(OnePlus2A2B2CProducer.GetSummaryHeaders());

            // Section-3 Title headers
            resultSummaryHeaders.AddRange(ThreeSaCostsSummary.GetHeaders());

            // Section-4 Title headers
            resultSummaryHeaders.AddRange(LaDataPrepCostsProducer.GetSummaryHeaders());

            // Section-5 Title headers
            resultSummaryHeaders.AddRange(SaSetupCostsSummary.GetHeaders());

            // Section Total bill headers
            resultSummaryHeaders.AddRange(TotalBillBreakdownProducer.GetSummaryHeaders());

            return resultSummaryHeaders;
        }

        public static List<CalcResultSummaryHeader> GetMaterialsBreakdownHeader(
            CalcResultSummary result,
            IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 1
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            // Add disposal fee summary header
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.DisposalFeeSummary,
                ColumnIndex = DisposalFeeSummaryColumnIndex,
            });

            var commsCostColumnIndex = MaterialsBreakdownHeaderCommsInitialColumnIndex;

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = commsCostColumnIndex,
                });
                commsCostColumnIndex = commsCostColumnIndex + (material.Code == MaterialCodes.Glass ?
                    MaterialsBreakdownHeaderCommsIncrementalColumnIndex + 1 :
                    MaterialsBreakdownHeaderCommsIncrementalColumnIndex);
            }

            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader
            {
                Name = CalcResultSummaryHeaders.CommsCostSummaryHeader,
                ColumnIndex = commsCostColumnIndex,
            });

            // Section-(1) & (2a)
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswoBadDebtprovision1, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor1, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswithBadDebtprovision1, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 2 }
            ]);

            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 7 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor2A, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 8 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A, decimalRoundUp)}", ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex + 9 }
            ]);

            // 2b comms total bill
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.CommsCostHeaderWithoutBadDebtFor2bTitle, decimalRoundUp)}", ColumnIndex = CommsCost2bColumnIndex },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.CommsCostHeaderBadDebtProvisionFor2bTitle,decimalRoundUp)}", ColumnIndex = CommsCost2bColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.CommsCostHeaderWithBadDebtFor2bTitle, decimalRoundUp)}", ColumnIndex = CommsCost2bColumnIndex + 2 },
             ]);

            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TwoCCommsCostsByCountryWithoutBadDebtProvision, CalcResultSummaryUtil.decimalRoundUp)}", ColumnIndex = TwoCCommsCostColumnIndex.Value },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TwoCBadDebtProvision, CalcResultSummaryUtil.decimalRoundUp)}",ColumnIndex = TwoCCommsCostColumnIndex.Value + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TwoCCommsCostsByCountryWithBadDebtProvision, CalcResultSummaryUtil.decimalRoundUp)}",ColumnIndex = TwoCCommsCostColumnIndex.Value + 2 }
            ]);

            // Section Total bill (1 + 2a + 2b + 2c)
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalOnePlus2A2B2CFeeWithBadDebtProvision, decimalRoundUp)}", ColumnIndex = OnePlus2A2B2CProducer.ColumnIndex },
            ]);

            //Section-3 -first header
            materialsBreakdownHeaders.AddRange([
               new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaOperatingCostsWoTitleSection3, decimalRoundUp)}", ColumnIndex = ThreeSaCostsSummary.ColumnIndex },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionTitleSection3, decimalRoundUp)}", ColumnIndex = ThreeSaCostsSummary.ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaOperatingCostsWithTitleSection3, decimalRoundUp)}", ColumnIndex = ThreeSaCostsSummary.ColumnIndex + 2 }
             ]);

            // LA data prep costs section 4
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.LaDataPrepCostsTitleSection4, decimalRoundUp)}", ColumnIndex = LaDataPrepCostsProducer.ColumnIndex },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.LaDataPrepCostsBadDebtProvisionTitleSection4, decimalRoundUp)}", ColumnIndex = LaDataPrepCostsProducer.ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.LaDataPrepCostsWithBadDebtProvisionTitleSection4, decimalRoundUp)}", ColumnIndex = LaDataPrepCostsProducer.ColumnIndex + 2 }
            ]);

            // Scheme administrator setup costs section 5
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaSetupCostsTitleSection5, decimalRoundUp)}", ColumnIndex = SaSetupCostsSummary.ColumnIndex },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaSetupCostsBadDebtProvisionTitleSection5, decimalRoundUp)}", ColumnIndex = SaSetupCostsSummary.ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaSetupCostsWithBadDebtProvisionTitleSection5, decimalRoundUp)}", ColumnIndex = SaSetupCostsSummary.ColumnIndex + 2 }
            ]);

            return materialsBreakdownHeaders;
        }

        public static List<CalcResultSummaryHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultSummaryHeader>();

            columnHeaders.AddRange([
                new () { Name = CalcResultSummaryHeaders.ProducerId },
                new () { Name = CalcResultSummaryHeaders.SubsidiaryId },
                new () { Name = CalcResultSummaryHeaders.ProducerOrSubsidiaryName },
                new () { Name = CalcResultSummaryHeaders.Level },
                new () { Name = CalcResultSummaryHeaders.ScaledupTonnages }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultSummaryHeader>
                {
                    new () { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage },
                    new () { Name = CalcResultSummaryHeaders.PublicBinTonnage },
                    new () { Name = CalcResultSummaryHeaders.TotalTonnage },
                    new () { Name = CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage },
                    new () { Name = CalcResultSummaryHeaders.NetTonnage },
                    new () { Name = CalcResultSummaryHeaders.PricePerTonne },
                    new () { Name = CalcResultSummaryHeaders.ProducerDisposalFee },
                    new () { Name = CalcResultSummaryHeaders.BadDebtProvision },
                    new () { Name = CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision },
                    new () { Name = CalcResultSummaryHeaders.EnglandWithBadDebtProvision },
                    new () { Name = CalcResultSummaryHeaders.WalesWithBadDebtProvision },
                    new () { Name = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision },
                    new () { Name = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision },
                };

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Insert(2, new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage });
            }

                columnHeaders.AddRange(columnHeadersList);
            }

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerDisposalFee },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerDisposalFeeWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotal }
            ]);

            foreach (var material in materials)
            {
                columnHeaders.AddRange([
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PricePerTonne },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision }
                ]);

                if (material.Code == MaterialCodes.Glass)
                {
                    int? index = columnHeaders.FindLastIndex(t => t.Name.Equals(CalcResultSummaryHeaders.PublicBinTonnage));
                    int t = (int)(index + 1);
                    columnHeaders.Insert(t, new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage });
            }
            }

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalBadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ]);

            // Section-(1) & (2a)
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswoBadDebtprovision, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionFor1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ]);

            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = DisposalFeeCommsCostsHeaderInitialColumnIndex },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionfor2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ]);

        // Percentage of Producer Tonnage vs All Producers
        columnHeaders.AddRange([
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PercentageofProducerTonnagevsAllProducers },
        ]);

            // 2b comms total
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerFeeWithoutBadDebtForComms2b, ColumnIndex = CommsCost2bColumnIndex },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionForComms2b },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerFeeForCommsCostsWithBadDebtForComms2b },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalWithBadDebtProvisionForComms2b },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalWithBadDebtProvisionForComms2b },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalWithBadDebtProvisionForComms2b },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalWithBadDebtProvisionForComms2b }
            ]);

            // 2c comms total
            columnHeaders.AddRange([
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithoutBadDebt, ColumnIndex = TwoCCommsCostColumnIndex.Value },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostBadDebtProvision },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostEnglandWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostWalesWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostScotlandWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostNIWithBadDebt }
            ]);

            // Section Total bill (1 + 2a + 2b + 2c)
            columnHeaders.AddRange(OnePlus2A2B2CProducer.GetHeaders());

            // SA operating cost section 3
            columnHeaders.AddRange(ThreeSaCostsProducer.GetHeaders());

            // Section-4 LA data prep costs column headers
            columnHeaders.AddRange(LaDataPrepCostsProducer.GetHeaders());

            // Section-5 SA setup costs column headers
            columnHeaders.AddRange(SaSetupCostsProducer.GetHeaders());

            // Section-TotalBill column headers
            columnHeaders.AddRange(TotalBillBreakdownProducer.GetHeaders());

            return columnHeaders;
        }

        public static decimal GetTotalProducerCommsFee(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal producerTotalCostWithoutBadDebtProvision = 0;

            foreach (var material in commsCostSummary)
            {
                producerTotalCostWithoutBadDebtProvision += material.Value.ProducerTotalCostWithoutBadDebtProvision;
            }

            return producerTotalCostWithoutBadDebtProvision;
        }

        public static decimal GetCommsTotalBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCostSummary)
        {
            decimal totalBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalBadDebtProvision += material.Value.BadDebtProvision;
            }

            return totalBadDebtProvision;
        }

        public static decimal GetTotalProducerCommsFeeWithBadDebtProvision(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal totalCommsCostsbyMaterialwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                totalCommsCostsbyMaterialwithBadDebtprovision += material.Value.ProducerTotalCostwithBadDebtProvision;
            }

            return totalCommsCostsbyMaterialwithBadDebtprovision;
        }

        public static decimal GetNorthernIrelandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                northernIrelandTotalwithBadDebtprovision += material.Value.NorthernIrelandWithBadDebtProvision;
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetScotlandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal scotlandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                scotlandTotalwithBadDebtprovision += material.Value.ScotlandWithBadDebtProvision;
            }

            return scotlandTotalwithBadDebtprovision;
        }

        public static decimal GetWalesCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal walesTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                walesTotalwithBadDebtprovision += material.Value.WalesWithBadDebtProvision;
            }

            return walesTotalwithBadDebtprovision;
        }

        public static decimal GetEnglandCommsTotal(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal englandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                englandTotalwithBadDebtprovision += material.Value.EnglandWithBadDebtProvision;
            }

            return englandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsCostHeaderWithoutBadDebtFor2bTitle(CalcResult calcResult)
        {
            return calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.ToList()[1].TotalValue;
        }

        public static decimal GetCommsCostHeaderBadDebtProvisionFor2bTitle(
            CalcResult calcResult,
            CalcResultSummary calcResultSummary)
        {
            var commsCost = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%')) / 100;
            return commsCost * badDebtProvision;
        }

        public static decimal GetCommsCostHeaderWithBadDebtFor2bTitle(CalcResultSummary calcResultSummary)
        {
            var commsCostHeaderWithoutBadDebt = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
            var commsCostHeaderBadDebtProvision = calcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle;
            return commsCostHeaderWithoutBadDebt + commsCostHeaderBadDebtProvision;
        }

        public static decimal GetCountryOnePlusFourApportionment(
            CalcResult calcResult,
            Countries country)
        {
            var onePlusFourApportionment = calcResult.CalcResultOnePlusFourApportionment
                .CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment);

            switch (country)
            {
                case Countries.England:
                    return onePlusFourApportionment.EnglandTotal;
                case Countries.Wales:
                    return onePlusFourApportionment.WalesTotal;
                case Countries.Scotland:
                    return onePlusFourApportionment.ScotlandTotal;
                case Countries.NorthernIreland:
                    return onePlusFourApportionment.NorthernIrelandTotal;
                default:
                    return 0;
            }
        }

        public static decimal GetParamsOtherFourCountryApportionmentPercentage(
            CalcResult calcResult,
            Countries country)
        {
            var fourCountryApportionment = calcResult.CalcResultParameterOtherCost.Details
                .SingleOrDefault(x => x.Name == CalcResultParameterOtherCostBuilder.FourCountryApportionmentPercentage);

            if (fourCountryApportionment == null)
            {
                return 0;
            }

            switch (country)
            {
                case Countries.England:
                    return fourCountryApportionment.EnglandValue;
                case Countries.Wales:
                    return fourCountryApportionment.WalesValue;
                case Countries.Scotland:
                    return fourCountryApportionment.ScotlandValue;
                case Countries.NorthernIreland:
                    return fourCountryApportionment.NorthernIrelandValue;
                default:
                    return 0;
            }
        }
    }
}
