namespace EPR.Calculator.Service.Function.Builder.Summary
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill;
    using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
    using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
    using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.ThreeSA;
    using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs;
    using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
    using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        private IEnumerable<CalcResultScaledupProducer> ScaledupProducers { get; set; }

        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
            this.ScaledupProducers = new List<CalcResultScaledupProducer>();
        }

        public async Task<CalcResultSummary> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            // Get and map materials from DB
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            this.ScaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers;

            var runProducerMaterialDetails = await (from pd in context.ProducerDetail
                                                    join prm in context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                                    where pd.CalculatorRunId == runId
                                                    select new CalcResultsProducerAndReportMaterialDetail
                                                    {
                                                        ProducerDetail = pd,
                                                        ProducerReportedMaterial = prm,
                                                    }).ToListAsync();
            var producerDetails = runProducerMaterialDetails.Select(x => x.ProducerDetail).Distinct().ToList();

            var orderedProducerDetails = GetOrderedListOfProducersAssociatedRunId(
                runId, producerDetails);

            // Household + PublicBin + HDC
            var TotalPackagingTonnage = GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, runId);

            var result = GetCalcResultSummary(
                orderedProducerDetails,
                materials,
                calcResult,
                TotalPackagingTonnage);

            return result;
        }

        public CalcResultSummary GetCalcResultSummary(
            IEnumerable<ProducerDetail> orderedProducerDetails,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<TotalPackagingTonnagePerRun> TotalPackagingTonnage)
        {
            var result = new CalcResultSummary();
            if (orderedProducerDetails.Any())
            {
                var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

                foreach (var producer in orderedProducerDetails)
                {
                    // We have to write an additional row if a producer have at least one subsidiary
                    // This additional row will be the total of this producer and its subsidiaries
                    var producersAndSubsidiaries = orderedProducerDetails.Where(pd => pd.ProducerId == producer.ProducerId);

                    // Make sure the total row is written only once
                    if (producersAndSubsidiaries.Count() > 1 &&
                        producerDisposalFees.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString()) == null)
                    {
                        var totalRow = GetProducerTotalRow(producersAndSubsidiaries.ToList(), materials, calcResult,
                            producerDisposalFees, false, TotalPackagingTonnage);
                        producerDisposalFees.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    producerDisposalFees.Add(GetProducerRow(producerDisposalFees, producer, materials, calcResult,
                        TotalPackagingTonnage));
                }


                // Calculate the total for all the producers
                var allTotalRow = GetProducerTotalRow(orderedProducerDetails.ToList(), materials, calcResult,
                    producerDisposalFees, true, TotalPackagingTonnage);
                producerDisposalFees.Add(allTotalRow);

                result.ProducerDisposalFees = producerDisposalFees;

                // Section-(1) & (2a)
                result.TotalFeeforLADisposalCostswoBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswoBadDebtprovision1(producerDisposalFees);
                result.BadDebtProvisionFor1 = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision1(producerDisposalFees);
                result.TotalFeeforLADisposalCostswithBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswithBadDebtprovision1(producerDisposalFees);

                result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswoBadDebtprovision2A(producerDisposalFees);
                result.BadDebtProvisionFor2A = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision2A(producerDisposalFees);
                result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswithBadDebtprovision2A(producerDisposalFees);

                // 2b comms total
                result.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
                result.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, result);
                result.CommsCostHeaderWithBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(result);

                TwoCCommsCostUtil.UpdateHeaderTotal(calcResult, result);

                // Section Total bill (1 + 2a + 2b + 2c)
                OnePlus2A2B2CProducer.SetValues(result);

                // Section-3 SA Operating costs section
                ThreeSaCostsProducer.GetProducerSetUpCostsSection3(calcResult, result);

                // Section-4 LA data prep costs
                LaDataPrepCostsProducer.SetValues(calcResult, result);

                // Section-5 SA setup costs
                SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, result);

                // Total bill section
                TotalBillBreakdownProducer.SetValues(result);
            }

            // Set headers with calculated column index
            CalcResultSummaryUtil.SetHeaders(result, materials);

            return result;
        }

        public static IEnumerable<ProducerDetail> GetOrderedListOfProducersAssociatedRunId(
            int runId,
            IEnumerable<ProducerDetail> producerDetails)
        {
            return producerDetails.Where(pd => pd.CalculatorRunId == runId).OrderBy(pd => pd.ProducerId).ToList();
        }

        public static IEnumerable<CalcResultsProducerAndReportMaterialDetail> GetProducerRunMaterialDetails(
            IEnumerable<ProducerDetail> producerDetails,
            IEnumerable<ProducerReportedMaterial> producerReportedmaterials,
            int runId)
        {
            return (from p in producerDetails
                    join m in producerReportedmaterials
                    on p.Id equals m.ProducerDetailId
                    where p.CalculatorRunId == runId
                    select new CalcResultsProducerAndReportMaterialDetail
                    {
                        ProducerDetail = p,
                        ProducerReportedMaterial = m,
                    }).ToList();
        }

        public CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
            List<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            bool isOverAllTotalRow,
            IEnumerable<TotalPackagingTonnagePerRun> TotalPackagingTonnage)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.Household, this.ScaledupProducers);
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.PublicBin, this.ScaledupProducers);

                materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    PublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnageTotal(producersAndSubsidiaries, material, this.ScaledupProducers),
                    ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.ConsumerWaste, this.ScaledupProducers),
                    NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnageTotal(producersAndSubsidiaries, material, this.ScaledupProducers),
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material,
                        calcResult, this.ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, Countries.England, this.ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, Countries.Wales, this.ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, Countries.Scotland, this.ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producersAndSubsidiaries, material,
                        calcResult, Countries.NorthernIreland, this.ScaledupProducers),
                });

                if (material.Code == MaterialCodes.Glass && materialCostSummary.TryGetValue(material, out var materialCost))
                {
                    materialCost.HouseholdDrinksContainersTonnage = CalcResultSummaryUtil.GetTonnageTotal(
                        producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, this.ScaledupProducers);
                }

                commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    ReportedPublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(producersAndSubsidiaries, material, this.ScaledupProducers),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(producersAndSubsidiaries, material,
                            calcResult, this.ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult, this.ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material,
                            calcResult, this.ScaledupProducers),
                });

                if (material.Code == MaterialCodes.Glass && commsCostSummary.TryGetValue(material, out var comm))
                {
                    comm.HouseholdDrinksContainers = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, this.ScaledupProducers);
                }
            }

            var totalRow = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
                ProducerName = isOverAllTotalRow
                    ? string.Empty
                    : producersAndSubsidiaries[0].ProducerName ?? string.Empty,
                SubsidiaryId = string.Empty,
                Level = isOverAllTotalRow ? string.Empty : CommonConstants.LevelOne.ToString(),
                isProducerScaledup = GetScaledupProducerStatusTotalRow(producersAndSubsidiaries[0], this.ScaledupProducers, isOverAllTotalRow),
                ProducerDisposalFeesByMaterial = materialCostSummary,

                // Disposal fee summary
                TotalProducerDisposalFee = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotal = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotal = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotal = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotal = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary),

                // For Comms Start
                TotalProducerCommsFee = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionComms = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalComms = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalComms = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalComms = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalComms = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),
                ProducerCommsFeesByMaterial = commsCostSummary,

                // Section-(1) & (2a)
                TotalProducerFeeforLADisposalCostswoBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvisionFor1 = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeforLADisposalCostswithBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotalWithBadDebtProvision = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary),

                TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvisionFor2A = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalWithBadDebtProvision2A = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),

                // Total Bill for 2b
                TotalProducerFeeWithoutBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),
                BadDebtProvisionFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),
                TotalProducerFeeWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),
                EnglandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),
                WalesTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),
                ScotlandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),
                NorthernIrelandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, TotalPackagingTonnage),

                // Section-3
                // Percentage of Producer Reported Tonnage vs All Producers
                PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producersAndSubsidiaries, TotalPackagingTonnage),

                isTotalRow = true,
            };

            TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow,
                producersAndSubsidiaries, TotalPackagingTonnage);

            return totalRow;
        }

        public CalcResultSummaryProducerDisposalFees GetProducerRow(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<TotalPackagingTonnagePerRun> TotalPackagingTonnage)
        {
            var materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            var result = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                Level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer).ToString(),
                isProducerScaledup = CalcResultSummaryUtil.IsProducerScaledup(producer, this.ScaledupProducers)
                    ? CommonConstants.ScaledupProducersYes
                    : CommonConstants.ScaledupProducersNo,
            };

            foreach (var material in materials)
            {
                var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, this.ScaledupProducers);
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin, this.ScaledupProducers);

                var calcResultSummaryProducerDisposalFeesByMaterial = new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    PublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnage(producer, material, this.ScaledupProducers),
                    ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.ConsumerWaste, this.ScaledupProducers),
                    NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material, this.ScaledupProducers),
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(producer, material, calcResult, this.ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(producer, material, calcResult, this.ScaledupProducers),
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult, this.ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, material, calcResult, Countries.England, this.ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, material, calcResult, Countries.Wales, this.ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, material, calcResult, Countries.Scotland, this.ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, material, calcResult, Countries.NorthernIreland, this.ScaledupProducers),
                };

                materialCostSummary.Add(material, calcResultSummaryProducerDisposalFeesByMaterial);

                if (material.Code == MaterialCodes.Glass && materialCostSummary.TryGetValue(material, out var producerDisposalFees))
                {
                    producerDisposalFees.HouseholdDrinksContainersTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, this.ScaledupProducers);
                }

                result.TotalProducerDisposalFee += calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFee;
                result.BadDebtProvision += calcResultSummaryProducerDisposalFeesByMaterial.BadDebtProvision;
                result.TotalProducerDisposalFeeWithBadDebtProvision += calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision;
                result.EnglandTotal += calcResultSummaryProducerDisposalFeesByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotal += calcResultSummaryProducerDisposalFeesByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotal += calcResultSummaryProducerDisposalFeesByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotal += calcResultSummaryProducerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision;

                var calcResultSummaryProducerCommsFeesCostByMaterial = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    ReportedPublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnage(producer, material, this.ScaledupProducers),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult, this.ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(producer, material, calcResult, this.ScaledupProducers),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult, this.ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult, this.ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(producer, material, calcResult, this.ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult, this.ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult, this.ScaledupProducers),
                };

                commsCostSummary.Add(material, calcResultSummaryProducerCommsFeesCostByMaterial);

                if (material.Code == MaterialCodes.Glass && commsCostSummary.TryGetValue(material, out var comm))
                {
                    comm.HouseholdDrinksContainers = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, this.ScaledupProducers);
                }

                result.TotalProducerCommsFee += calcResultSummaryProducerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
                result.BadDebtProvisionComms += calcResultSummaryProducerCommsFeesCostByMaterial.BadDebtProvision;
                result.TotalProducerCommsFeeWithBadDebtProvision += calcResultSummaryProducerCommsFeesCostByMaterial.ProducerTotalCostwithBadDebtProvision;
                result.EnglandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.NorthernIrelandWithBadDebtProvision;
            }

            result.ProducerDisposalFeesByMaterial = materialCostSummary;
            result.ProducerCommsFeesByMaterial = commsCostSummary;

            // Section-(1) & (2a)
            result.TotalProducerFeeforLADisposalCostswoBadDebtprovision = result.TotalProducerDisposalFee;
            result.BadDebtProvisionFor1 = result.BadDebtProvision;
            result.TotalProducerFeeforLADisposalCostswithBadDebtprovision = result.TotalProducerDisposalFeeWithBadDebtProvision;
            result.EnglandTotalWithBadDebtProvision = result.EnglandTotal;
            result.WalesTotalWithBadDebtProvision = result.WalesTotal;
            result.ScotlandTotalWithBadDebtProvision = result.ScotlandTotal;
            result.NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotal;

            result.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision = result.TotalProducerCommsFee;
            result.BadDebtProvisionFor2A = result.BadDebtProvisionComms;
            result.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision = result.TotalProducerCommsFeeWithBadDebtProvision;
            result.EnglandTotalWithBadDebtProvision2A = result.EnglandTotalComms;
            result.WalesTotalWithBadDebtProvision2A = result.WalesTotalComms;
            result.ScotlandTotalWithBadDebtProvision2A = result.ScotlandTotalComms;
            result.NorthernIrelandTotalWithBadDebtProvision2A = result.NorthernIrelandTotalComms;

            // Section-3
            // Percentage of Producer Reported Tonnage vs All Producers
            result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, TotalPackagingTonnage);

            // Total Bill for 2b
            result.TotalProducerFeeWithoutBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, TotalPackagingTonnage);
            result.BadDebtProvisionFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, TotalPackagingTonnage);
            result.TotalProducerFeeWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, TotalPackagingTonnage);
            result.EnglandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, TotalPackagingTonnage);
            result.WalesTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, TotalPackagingTonnage);
            result.ScotlandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, TotalPackagingTonnage);
            result.NorthernIrelandTotalWithBadDebtFor2bComms = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, TotalPackagingTonnage);

            TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, TotalPackagingTonnage);

            return result;
        }

        public static IEnumerable<TotalPackagingTonnagePerRun> GetTotalPackagingTonnagePerRun(
           IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults,
           IEnumerable<MaterialDetail> materials,
           int runId)
        {
            var allProducerDetails = allResults.Select(x => x.ProducerDetail).Distinct();
            var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterial);

            var result =
           (from p in allProducerDetails
            join pm in allProducerReportedMaterials on p.Id equals pm.ProducerDetailId
            join m in materials on pm.MaterialId equals m.Id
            where p.CalculatorRunId == runId &&
             (
         pm.PackagingType == PackagingTypes.Household || pm.PackagingType == PackagingTypes.PublicBin ||
                 (
                     pm.PackagingType == PackagingTypes.HouseholdDrinksContainers && m.Code == MaterialCodes.Glass
              )
             )
            group new { m = pm, p } by new { p.ProducerId, p.SubsidiaryId } into g
            select new TotalPackagingTonnagePerRun
            {
                ProducerId = g.Key.ProducerId,
                SubsidiaryId = g.Key.SubsidiaryId,
                TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage),
            }).ToList();

            return result;
        }

        private static string GetScaledupProducerStatusTotalRow(
            ProducerDetail producer,
            IEnumerable<CalcResultScaledupProducer> scaledupProducers,
            bool isOverAllTotalRow)
        {
            if (isOverAllTotalRow)
            {
                return CommonConstants.Totals;
            }

            return CalcResultSummaryUtil.IsProducerScaledup(producer, scaledupProducers)
                ? CommonConstants.ScaledupProducersYes
                : CommonConstants.ScaledupProducersNo;
        }
    }
}