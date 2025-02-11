﻿using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Data.DataModels;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.ScaledupProducers
{
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

        public async Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto,
            CalcResult calcResult)
        {
            try
            {
                var runId = resultsRequestDto.RunId;
                var materialsFromDb = await context.Material.ToListAsync();
                var materials = MaterialMapper.Map(materialsFromDb);

                var scaledupProducersSummary = new CalcResultScaledupProducers();
                var scaledupProducers = new List<CalcResultScaledupProducer>();

                var scaleupOrganisationIds = await GetScaledUpOrganisationIds(resultsRequestDto.RunId);
                if (scaleupOrganisationIds.Any() && scaleupOrganisationIds != null)
                {
                    List<int> organisationIds = scaleupOrganisationIds.Select(x => x.OrganisationId).ToList();
                    var runProducerMaterialDetails = await (from run in context.CalculatorRuns
                                                            join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                                            join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                                            join spl in context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                                            join pd in context.ProducerDetail.Include(x => x.ProducerReportedMaterials) on crpdd.OrganisationId equals pd.ProducerId
                                                            where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                                                            select new CalcResultScaledupProducer
                                                            {
                                                                ProducerId = pd.ProducerId,
                                                                SubsidiaryId = pd.SubsidiaryId,
                                                                ProducerName = pd.ProducerName,
                                                                ScaleupFactor = spl.ScaleupFactor,
                                                                SubmissonPeriodCode = spl.SubmissionPeriod,
                                                                DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                                                                DaysInWholePeriod = spl.DaysInSubmissionPeriod,
                                                                Level = pd.SubsidiaryId != null ? CommonConstants.LevelTwo.ToString() : CommonConstants.LevelOne.ToString(),
                                                                isTotalRow = false,
                                                            }).Distinct().ToListAsync();

                    var allOrganisationPomDetails = await (from run in context.CalculatorRuns
                                                           join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                                           join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                                           where run.Id == runId && organisationIds.Contains(crpdd.OrganisationId.GetValueOrDefault())
                                                           select crpdd).Distinct().ToListAsync();

                    //foreach(var runProducerMaterialDetail in runProducerMaterialDetails)
                    //{
                    //    if (runProducerMaterialDetail)
                    //}

                    var level2Rows = runProducerMaterialDetails.Where(x => string.IsNullOrEmpty(x.SubsidiaryId)).GroupBy(x => new { x.ProducerId, x.SubsidiaryId }).ToList();

                    foreach (var row in level2Rows)
                    {
                        if (runProducerMaterialDetails.Any(x => x.ProducerId == row.Key.ProducerId && x.SubsidiaryId != null)
                            && 
                            row.Count() > 0)
                        {
                            var levelRows = runProducerMaterialDetails.Where(x => x.ProducerId == row.Key.ProducerId && string.IsNullOrEmpty(x.SubsidiaryId));
                            foreach (var level2Row in levelRows)
                            {
                                level2Row.Level = CommonConstants.LevelTwo.ToString();
                            }
                        }
                    }

                    var groupByResult = runProducerMaterialDetails
                        .Where(x => x.SubsidiaryId != null)
                        .GroupBy(x => new { x.ProducerId, x.SubmissonPeriodCode })
                        .Where(x => x.Count() > 1)
                        .ToList();

                    //var abc = runProducerMaterialDetails
                    //    .GroupBy(x => new { x.ProducerId, x.SubsidiaryId })
                    //    .Where(x => x.Count() > 1);


                    foreach (var pair in groupByResult)
                    {
                        var first = pair.ToList().First();

                        // We are always expecting record with subsidiaryid null
                        var parentProducer = runProducerMaterialDetails.Where(x => x.ProducerId == pair.Key.ProducerId && x.SubsidiaryId == null).ToList();

                        var extraRow = new CalcResultScaledupProducer
                        {
                            ProducerId = pair.Key.ProducerId,
                            SubsidiaryId = string.Empty,
                            ProducerName = "extra" + parentProducer[0].ProducerName,
                            ScaleupFactor = first.ScaleupFactor,
                            SubmissonPeriodCode = pair.Key.SubmissonPeriodCode,
                            DaysInSubmissionPeriod = first.DaysInSubmissionPeriod,
                            DaysInWholePeriod = first.DaysInSubmissionPeriod,
                            Level = CommonConstants.LevelOne.ToString(),
                            isTotalRow = true,
                            // ScaledupProducerTonnageByMaterial = GetTonnages(pomData, materials, first.SubmissonPeriodCode, first.ScaleupFactor)
                        };

                        runProducerMaterialDetails.Add(extraRow);

                        // scaledupProducers.Add(extraRow);
                    }

                    foreach (var item in runProducerMaterialDetails)
                    {
                        var pomData = item.isTotalRow
                            ? allOrganisationPomDetails
                                .Where(pom => pom.OrganisationId == item.ProducerId && pom.SubmissionPeriod == item.SubmissonPeriodCode)
                                .ToList()
                            : allOrganisationPomDetails
                                .Where(pom => pom.OrganisationId == item.ProducerId && pom.SubsidaryId == item.SubsidiaryId && pom.SubmissionPeriod == item.SubmissonPeriodCode)
                                .ToList();

                        item.ScaledupProducerTonnageByMaterial = GetTonnages(pomData, materials, item.SubmissonPeriodCode, item.ScaleupFactor);
                    }



                    var orderedRunProducerMaterialDetails = runProducerMaterialDetails
                        .OrderBy(p => p.ProducerId)
                        .OrderBy(p => p.Level)
                        .ToList();

                    scaledupProducersSummary.ScaledupProducers = orderedRunProducerMaterialDetails;
                }

                SetHeaders(scaledupProducersSummary, materials);
                return scaledupProducersSummary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<ScaleupProducer>> GetScaledUpOrganisationIds(int runId)
        {
            var scaleupProducerIds = await (from run in context.CalculatorRuns
                                            join crpdm in context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals crpdm.Id
                                            join crpdd in context.CalculatorRunPomDataDetails on crpdm.Id equals crpdd.CalculatorRunPomDataMasterId
                                            join spl in context.SubmissionPeriodLookup on crpdd.SubmissionPeriod equals spl.SubmissionPeriod
                                            where run.Id == runId && crpdd.OrganisationId != null && spl.ScaleupFactor > NormalScaleup
                                            select new ScaleupProducer
                                            {
                                                OrganisationId = crpdd.OrganisationId.GetValueOrDefault(),
                                                ScaleupFactor = spl.ScaleupFactor,
                                                SubmissionPeriod = spl.SubmissionPeriod,
                                                DaysInSubmissionPeriod = spl.DaysInSubmissionPeriod,
                                                DaysInWholePeriod = spl.DaysInWholePeriod,
                                            })
                               .Distinct().ToListAsync();
            return scaleupProducerIds == null ? new List<ScaleupProducer>() : scaleupProducerIds;
        }

        private static int GetLevelIndex(IEnumerable<CalcResultScaledupProducer> scaledupProducers, CalcResultScaledupProducer producer)
        {
            if (producer.isTotalRow)
            {
                return CommonConstants.LevelOne;
            }

            if (producer.SubsidiaryId == null && scaledupProducers.Where(x => x.ProducerId == producer.ProducerId).Count() == 1)
            {
                return CommonConstants.LevelTwo;
            }

            return 1;
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetTonnages(IEnumerable<CalculatorRunPomDataDetail> pomData,
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
                    .Sum(pom => pom.PackagingMaterialWeight);

                scaledupProducerTonnage.ReportedPublicBinTonnage = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.PublicBin)
                    .Sum(pom => pom.PackagingMaterialWeight);

                scaledupProducerTonnage.TotalReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                    scaledupProducerTonnage.ReportedPublicBinTonnage;

                var hdc = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.HouseholdDrinksContainers)
                    .Sum(pom => pom.PackagingMaterialWeight);

                scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage = (decimal)materialPomData
                    .Where(pom => pom.PackagingType == PackagingTypes.ConsumerWaste)
                    .Sum(pom => pom.PackagingMaterialWeight);

                scaledupProducerTonnage.NetReportedTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage +
                    scaledupProducerTonnage.ReportedPublicBinTonnage +
                    hdc;

                scaledupProducerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage = scaledupProducerTonnage.ReportedHouseholdPackagingWasteTonnage * scaleUpFactor;
                scaledupProducerTonnage.ScaledupReportedPublicBinTonnage = scaledupProducerTonnage.ReportedPublicBinTonnage * scaleUpFactor;
                scaledupProducerTonnage.ScaledupTotalReportedTonnage = scaledupProducerTonnage.TotalReportedTonnage * scaleUpFactor;
                scaledupProducerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage = scaledupProducerTonnage.ReportedSelfManagedConsumerWasteTonnage * scaleUpFactor;
                scaledupProducerTonnage.ScaledupNetReportedTonnage = scaledupProducerTonnage.NetReportedTonnage * scaleUpFactor;

                scaledupProducerTonnages.Add(material.Code, scaledupProducerTonnage);
            }
            return scaledupProducerTonnages;
        }

        private static void SetHeaders(CalcResultScaledupProducers producers, IEnumerable<MaterialDetail> materials)
        {
            producers.TitleHeader = new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ColumnIndex = 1
            };

            producers.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(materials);

            producers.ColumnHeaders = GetColumnHeaders(materials);
        }

        private static List<CalcResultScaledupProducerHeader> GetMaterialsBreakdownHeader(IEnumerable<MaterialDetail> materials)
        {
            var materialsBreakdownHeaders = new List<CalcResultScaledupProducerHeader>();
            var columnIndex = MaterialsBreakdownHeaderInitialColumnIndex;

            materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
            {
                Name = CalcResultScaledupProducerHeaders.EachSubmissionForTheYear,
                ColumnIndex = 1
            });

            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultScaledupProducerHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });

                columnIndex = material.Code == MaterialCodes.Glass
                    ? columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex + 1
                    : columnIndex + MaterialsBreakdownHeaderIncrementalColumnIndex;
            }

            return materialsBreakdownHeaders;
        }

        private static List<CalcResultScaledupProducerHeader> GetColumnHeaders(IEnumerable<MaterialDetail> materials)
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

                columnHeaders.AddRange(columnHeadersList);
            }

            return columnHeaders;
        }

        private static int GetLevelIndex(IEnumerable<CalcResultScaledupProducer> scaledupProducers, ProducerDetail producer)
        {
            var totalRow = scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId && p.isTotalRow);

            return totalRow == null ? (int)CalcResultSummaryLevelIndex.One : (int)CalcResultSummaryLevelIndex.Two;
        }
    }
}
