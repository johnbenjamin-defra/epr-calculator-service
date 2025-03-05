namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private const decimal NormalScaleup = 1.0M;
        private const int MaterialsBreakdownHeaderInitialColumnIndex = 9;
        private const int MaterialsBreakdownHeaderIncrementalColumnIndex = 10;

        private readonly ApplicationDBContext context;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public static CalcResultScaledupProducer GetOverallTotalRow(
            IEnumerable<CalcResultScaledupProducer> orderedRunProducerMaterialDetails,
            IEnumerable<MaterialDetail> materials)
        {
            var overallTotalRow = new CalcResultScaledupProducer
            {
                IsTotalRow = true,
                ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>(),
            };

            var allMaterialDict = orderedRunProducerMaterialDetails.Where(x => !x.IsSubtotalRow).Select(x => x.ScaledupProducerTonnageByMaterial);
            foreach (var material in materials)
            {
                var totalRow = new CalcResultScaledupProducerTonnage();
                var materialValues = allMaterialDict.Where(x => x.ContainsKey(material.Code)).Select(x => x[material.Code]).ToList();
                totalRow.ReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ReportedHouseholdPackagingWasteTonnage);
                totalRow.ReportedPublicBinTonnage = materialValues.Sum(x => x.ReportedPublicBinTonnage);
                if (material.Code == MaterialCodes.Glass)
                {
                    totalRow.HouseholdDrinksContainersTonnageGlass = materialValues.Sum(x => x.HouseholdDrinksContainersTonnageGlass);
                }

                totalRow.TotalReportedTonnage = materialValues.Sum(x => x.TotalReportedTonnage);
                totalRow.ReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ReportedSelfManagedConsumerWasteTonnage);
                totalRow.NetReportedTonnage = materialValues.Sum(x => x.NetReportedTonnage);
                totalRow.ScaledupReportedHouseholdPackagingWasteTonnage = materialValues.Sum(x => x.ScaledupReportedHouseholdPackagingWasteTonnage);
                totalRow.ScaledupReportedPublicBinTonnage = materialValues.Sum(x => x.ScaledupReportedPublicBinTonnage);
                if (material.Code == MaterialCodes.Glass)
                {
                    totalRow.ScaledupHouseholdDrinksContainersTonnageGlass = materialValues.Sum(x => x.ScaledupHouseholdDrinksContainersTonnageGlass);
                }

                totalRow.ScaledupTotalReportedTonnage = materialValues.Sum(x => x.ScaledupTotalReportedTonnage);
                totalRow.ScaledupReportedSelfManagedConsumerWasteTonnage = materialValues.Sum(x => x.ScaledupReportedSelfManagedConsumerWasteTonnage);
                totalRow.ScaledupNetReportedTonnage = materialValues.Sum(x => x.ScaledupNetReportedTonnage);

                overallTotalRow.ScaledupProducerTonnageByMaterial.Add(material.Name, totalRow);
            }

            return overallTotalRow;
        }

        public static void CalculateScaledupTonnage(
            IEnumerable<CalcResultScaledupProducer> runProducerMaterialDetails,
            IEnumerable<CalculatorRunPomDataDetail> allOrganisationPomDetails,
            IEnumerable<MaterialDetail> materials)
        {
            foreach (var item in runProducerMaterialDetails)
            {
                var pomData = item.IsSubtotalRow
                    ? allOrganisationPomDetails
                        .Where(pom => pom.OrganisationId == item.ProducerId && pom.SubmissionPeriod == item.SubmissionPeriodCode)
                        .ToList()
                    : allOrganisationPomDetails
                        .Where(pom => pom.OrganisationId == item.ProducerId && pom.SubsidaryId == item.SubsidiaryId && pom.SubmissionPeriod == item.SubmissionPeriodCode)
                        .ToList();

                item.ScaledupProducerTonnageByMaterial = GetTonnages(pomData, materials, item.SubmissionPeriodCode!, item.ScaleupFactor!);
            }
        }

        public static void AddExtraRows(List<CalcResultScaledupProducer> runProducerMaterialDetails)
        {
            var level2Rows = runProducerMaterialDetails
                .Where(x => string.IsNullOrEmpty(x.SubsidiaryId))
                .GroupBy(x => new { x.ProducerId, x.SubsidiaryId }).ToList();

            foreach (var row in level2Rows)
            {
                if (runProducerMaterialDetails.Exists(x => x.ProducerId == row.Key.ProducerId && x.SubsidiaryId != null && x.SubsidiaryId != string.Empty)
                    &&
                    row.Any())
                {
                    var levelRows = runProducerMaterialDetails.Where(x => x.ProducerId == row.Key.ProducerId && string.IsNullOrEmpty(x.SubsidiaryId));
                    foreach (var level2Row in levelRows)
                    {
                        level2Row.Level = CommonConstants.LevelTwo.ToString();
                    }
                }
            }

            var groupByResult = runProducerMaterialDetails
                .Where(x => x.SubsidiaryId != null && x.SubsidiaryId != string.Empty)
                .GroupBy(x => new { x.ProducerId, x.SubmissionPeriodCode })
                .Where(x => x.Count() > 1)
                .ToList();

            foreach (var pair in groupByResult)
            {
                var first = pair.ToList()[0];

                // We are always expecting record with subsidiaryid null
                var parentProducer = runProducerMaterialDetails.Where(x => x.ProducerId == pair.Key.ProducerId && x.SubsidiaryId == null).ToList();

                var extraRow = new CalcResultScaledupProducer
                {
                    ProducerId = pair.Key.ProducerId,
                    SubsidiaryId = string.Empty,
                    ProducerName = parentProducer[0].ProducerName,
                    ScaleupFactor = first.ScaleupFactor,
                    SubmissionPeriodCode = pair.Key.SubmissionPeriodCode,
                    DaysInSubmissionPeriod = first.DaysInSubmissionPeriod,
                    DaysInWholePeriod = first.DaysInWholePeriod,
                    Level = CommonConstants.LevelOne.ToString(),
                    IsSubtotalRow = true,
                };

                runProducerMaterialDetails.Add(extraRow);
            }
        }

        /// <inheritdoc/>
        public async Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await this.context.Material.ToListAsync();
            var materials = MaterialMapper.Map(materialsFromDb);

            List<CalcResultScaledupProducer> orderedRunProducerMaterialDetails = new List<CalcResultScaledupProducer>();

            var organisationIds = await this.GetScaledUpOrganisationIdsAsync(resultsRequestDto.RunId);
            if (organisationIds != null && organisationIds.Any())
            {
                var runProducerMaterialDetails = await this.GetProducerReportedMaterialsAsync(runId, organisationIds);

                var allOrganisationPomDetails = await this.GetScaledupOrganisationDetails(runId, organisationIds);

                AddExtraRows(runProducerMaterialDetails);

                CalculateScaledupTonnage(runProducerMaterialDetails, allOrganisationPomDetails, materials);

                orderedRunProducerMaterialDetails = runProducerMaterialDetails
                    .OrderBy(p => p.ProducerId)
                    .ThenBy(p => p.Level)
                    .ThenBy(p => p.SubsidiaryId)
                    .ThenBy(p => p.SubmissionPeriodCode)
                    .ToList();

                var overallTotalRow = GetOverallTotalRow(orderedRunProducerMaterialDetails, materials);

                orderedRunProducerMaterialDetails.Add(overallTotalRow);
            }

            var scaledupProducersSummary = new CalcResultScaledupProducers
            {
                ScaledupProducers = orderedRunProducerMaterialDetails,
            };

            SetHeaders(scaledupProducersSummary, materials);
            return scaledupProducersSummary;
        }

        public async Task<IEnumerable<CalculatorRunPomDataDetail>> GetScaledupOrganisationDetails(int runId, IEnumerable<int> organisationIds)
        {
            var result = await (from run in this.context.CalculatorRuns
                                join crpdm in this.context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                join crpdd in this.context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                                select crpdd).Distinct().ToListAsync();
            return result;
        }

        public async Task<List<CalcResultScaledupProducer>> GetProducerReportedMaterialsAsync(int runId, IEnumerable<int> organisationIds)
        {
            var result = await (from run in this.context.CalculatorRuns
                                join crpdm in this.context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                join crpdd in this.context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                join spl in this.context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                join pd in this.context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crpdd.OrganisationId equals pd.ProducerId
                                where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                                select new CalcResultScaledupProducer
                                {
                                    ProducerId = pd.ProducerId,
                                    SubsidiaryId = pd.SubsidiaryId!,
                                    ProducerName = pd.ProducerName!,
                                    ScaleupFactor = spl.ScaleupFactor,
                                    SubmissionPeriodCode = spl.SubmissionPeriod,
                                    DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                                    DaysInWholePeriod = spl.DaysInWholePeriod,
                                    Level = !string.IsNullOrEmpty(pd.SubsidiaryId) ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                                }).Distinct().ToListAsync();
            return result ?? new List<CalcResultScaledupProducer>();
        }

        public async Task<IEnumerable<int>> GetScaledUpOrganisationIdsAsync(int runId)
        {
            var scaleupProducerIds = await (from run in this.context.CalculatorRuns
                                            join crpdm in this.context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                            join crpdd in this.context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                            join spl in this.context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                            where run.Id == runId && crpdd.OrganisationId != null && spl.ScaleupFactor > NormalScaleup
                                            select crpdd.OrganisationId.GetValueOrDefault()).Distinct().ToListAsync();
            return scaleupProducerIds ?? [];
        }

        public static Dictionary<string, CalcResultScaledupProducerTonnage> GetTonnages(IEnumerable<CalculatorRunPomDataDetail> pomData,
            IEnumerable<MaterialDetail> materials,
            string submissionPeriod,
            decimal scaleUpFactor)
        {
            var scaledupProducerTonnages = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            foreach (var material in materials)
            {
                var scaledupProducerTonnage = new CalcResultScaledupProducerTonnage();

                var materialPomData = pomData.Where(pom => pom.PackagingMaterial == material.Code && pom.SubmissionPeriod == submissionPeriod);

                scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.Household)
                    .Sum(pom => CommonUtil.ConvertKilogramToTonne(pom.PackagingMaterialWeight ?? 0));

                scaledupProducerTonnage.ReportedPublicBinTonnage = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.PublicBin)
                    .Sum(pom => CommonUtil.ConvertKilogramToTonne(pom.PackagingMaterialWeight ?? 0));

                var hdc = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.HouseholdDrinksContainers)
                    .Sum(pom => CommonUtil.ConvertKilogramToTonne(pom.PackagingMaterialWeight ?? 0));

                if (material.Code == MaterialCodes.Glass)
                {
                    scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass = hdc;
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                                            scaledupProducerTonnage.ReportedPublicBinTonnage + scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass;
                }
                else
                {
                    scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                    scaledupProducerTonnage.ReportedPublicBinTonnage;
                }

                scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.ConsumerWaste)
                    .Sum(pom => CommonUtil.ConvertKilogramToTonne(pom.PackagingMaterialWeight ?? 0));

                scaledupProducerTonnage.NetReportedTonnage = scaledupProducerTonnage.TotalReportedTonnage - scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage;
                scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = Math.Round(scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage * scaleUpFactor, 3);
                scaledupProducerTonnage.ScaledupReportedPublicBinTonnage = Math.Round(scaledupProducerTonnage.ReportedPublicBinTonnage * scaleUpFactor, 3);
                if (material.Code == MaterialCodes.Glass)
                {
                    scaledupProducerTonnage.ScaledupHouseholdDrinksContainersTonnageGlass = scaledupProducerTonnage.HouseholdDrinksContainersTonnageGlass * scaleUpFactor;
                }

                scaledupProducerTonnage.ScaledupTotalReportedTonnage = Math.Round(scaledupProducerTonnage.TotalReportedTonnage * scaleUpFactor, 3);
                scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = Math.Round(scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage * scaleUpFactor, 3);
                scaledupProducerTonnage.ScaledupNetReportedTonnage = Math.Round(scaledupProducerTonnage.NetReportedTonnage * scaleUpFactor, 3);
                scaledupProducerTonnages.Add(material.Code, scaledupProducerTonnage);
            }

            return scaledupProducerTonnages;
        }

        public static void SetHeaders(CalcResultScaledupProducers producers, IEnumerable<MaterialDetail> materials)
        {
            producers.TitleHeader = new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ColumnIndex = 1,
            };

            producers.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials);

            producers.ColumnHeaders = GetColumnHeaders(materials);
        }

        public static List<CalcResultScaledupProducerHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultScaledupProducerHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1,
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex,
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 2
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders;
        }

        public static List<CalcResultScaledupProducerHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
        {
            var columnHeaders = new List<CalcResultScaledupProducerHeader>();

            columnHeaders.AddRange([
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubsidiaryId },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ProducerOrSubsidiaryName },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.Level },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.SubmissionPeriodCode },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInSubmissionPeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.DaysInWholePeriod },
                new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaleupFactor }
            ]);

            foreach (var material in materials)
            {
                var columnHeadersList = new List<CalcResultScaledupProducerHeader>
                {
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ReportedHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ReportedPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.TotalReportedTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.NetReportedTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedHouseholdPackagingWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedPublicBinTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupTotalReportedTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupReportedSelfManagedConsumerWasteTonnage },
                    new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupNetReportedTonnage },
                };

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Insert(2, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.HouseholdDrinksContainersTonnageGlass });
                    columnHeadersList.Insert(8, new CalcResultScaledupProducerHeader { Name = CalcResultScaledupProducerHeaders.ScaledupHouseholdDrinksContainersTonnageGlass });
                }

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders;
        }
    }
}