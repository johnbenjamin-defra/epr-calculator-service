using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System;
using EPR.Calculator.Service.Function.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.LaDisposalCost
{
    public class CalcRunLaDisposalCostBuilder : ICalcRunLaDisposalCostBuilder
    {
        private const string EmptyString = "0";

        internal class ProducerData
        {
            public required string MaterialName { get; set; }

            public required string PackagingType { get; set; }

            public decimal Tonnage { get; set; }

            public ProducerDetail ProducerDetail { get; set; }
        }

        private readonly ApplicationDBContext context;
        private List<ProducerData> producerData;

        public CalcRunLaDisposalCostBuilder(ApplicationDBContext context)
        {
            this.context = context;
            this.producerData = new List<ProducerData>();
        }

        public async Task<CalcResultLaDisposalCostData> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();
            var orderId = 1;

            await this.SetProducerData(resultsRequestDto);

            var scaledUpProducerReportedOn = calcResult.CalcResultScaledupProducers.ScaledupProducers.First(x => x.IsTotalRow);
            this.producerData = this.producerData.Where(t => !calcResult.CalcResultScaledupProducers.ScaledupProducers.Any(i => i.ProducerId == t?.ProducerDetail.ProducerId)).ToList();

            var lapcapDetails = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails
                .Where(t => t.OrderId != 1 && t.Name != CalcResultLapcapDataBuilder.CountryApportionment).ToList();

            foreach (var details in lapcapDetails)
            {
                var laDiposalDetail = new CalcResultLaDisposalCostDataDetail()
                {
                    Name = details.Name,
                    England = details.EnglandDisposalCost,
                    Wales = details.WalesDisposalCost,
                    Scotland = details.ScotlandDisposalCost,
                    NorthernIreland = details.NorthernIrelandDisposalCost,
                    Total = details.TotalDisposalCost,
                    ProducerReportedHouseholdPackagingWasteTonnage = this.GetTonnageDataByMaterial(details.Name, scaledUpProducerReportedOn),
                    ReportedPublicBinTonnage = this.GetReportedPublicBinTonnage(details.Name, scaledUpProducerReportedOn),
                    HouseholdDrinkContainers = this.GetReportedHouseholdDrinksContainerTonnage(details.Name, scaledUpProducerReportedOn),
                    OrderId = ++orderId,
                };
                laDiposalDetail.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(laDiposalDetail.Name, calcResult.CalcResultLateReportingTonnageData.CalcResultLateReportingTonnageDetails.ToList());
                laDiposalDetail.ProducerReportedTotalTonnage = GetProducerReportedTotalTonnage(laDiposalDetail);
                laDiposalDetail.DisposalCostPricePerTonne = laDiposalDetail.Name == CommonConstants.Total
                    ? string.Empty
                    : CalculateDisposalCostPricePerTonne(laDiposalDetail);
                laDisposalCostDetails.Add(laDiposalDetail);
            }

            var header = GetHeader();
            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData()
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable(),
            };
        }

        private string GetReportedHouseholdDrinksContainerTonnage(string materialName, CalcResultScaledupProducer scaledUpProducerReportedOn)
        {
            // Return an empty string if the material name is not "Glass" or "Total"
            if (materialName != MaterialNames.Glass && materialName != CommonConstants.Total)
            {
                return string.Empty;
            }

            scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.TryGetValue(materialName, out var scaledProducerTonnages);

            decimal producerDataTotal = materialName == CommonConstants.Total
                ? this.producerData.Where(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage)
                : this.producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.Tonnage);

            decimal scaledDataTotal = materialName == CommonConstants.Total
                ? scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.Values.Sum(t => t.ScaledupHouseholdDrinksContainersTonnageGlass)
                : (scaledProducerTonnages?.ScaledupHouseholdDrinksContainersTonnageGlass ?? 0);

            // Return "0" if the material is "Glass" and there's no data, otherwise return the total tonnage as a string
            return (materialName == MaterialNames.Glass && (producerDataTotal + scaledDataTotal) == 0) ? EmptyString : (producerDataTotal + scaledDataTotal).ToString();
        }

        private string GetReportedPublicBinTonnage(string materialName, CalcResultScaledupProducer scaledUpProducerReportedOn)
        {
            scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.TryGetValue(materialName, out var scaledProducerTonnages);

            decimal producerDataTotal = materialName == CommonConstants.Total
                ? this.producerData.Where(p => p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage)
                : this.producerData.Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.Tonnage);

            decimal scaledDataTotal = materialName == CommonConstants.Total
                ? scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.Values.Sum(t => t.ScaledupReportedPublicBinTonnage)
                : (scaledProducerTonnages?.ScaledupReportedPublicBinTonnage ?? 0);

            return (producerDataTotal + scaledDataTotal).ToString();
        }

        private string GetTonnageDataByMaterial(string materialName, CalcResultScaledupProducer scaledUpProducerReportedOn)
        {
            scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.TryGetValue(materialName, out var scaledProducerTonnages);

            decimal producerDataTotal = materialName == CommonConstants.Total
                ? this.producerData.Where(t => t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage)
                : this.producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage);

            decimal scaledDataTotal = materialName == CommonConstants.Total
                ? scaledUpProducerReportedOn.ScaledupProducerTonnageByMaterial.Values.Sum(t => t.ScaledupReportedHouseholdPackagingWasteTonnage)
                : (scaledProducerTonnages?.ScaledupReportedHouseholdPackagingWasteTonnage ?? 0);

            return (producerDataTotal + scaledDataTotal).ToString();
        }

        private static string GetLateReportingTonnageDataByMaterial(string materialName, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details.Where(t => t.Name == materialName).Sum(t => t.TotalLateReportingTonnage).ToString();
        }

        private static string GetProducerReportedTotalTonnage(CalcResultLaDisposalCostDataDetail detail)
        {
            var householdDrinkContainersValue = detail.HouseholdDrinkContainers == null
                ? 0
                : GetDecimalValue(detail.HouseholdDrinkContainers);

            var value = GetDecimalValue(detail.LateReportingTonnage)
                + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage)
                + GetDecimalValue(detail.ReportedPublicBinTonnage)
                + householdDrinkContainersValue;

            return value.ToString();
        }

        private static string CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var householdTonnagePlusLateReportingTonnage = GetDecimalValue(detail.ProducerReportedTotalTonnage);
            if (householdTonnagePlusLateReportingTonnage == 0)
            {
                return EmptyString;
            }

            var value = Math.Round(ConvertCurrencyToDecimal(detail.Total) / householdTonnagePlusLateReportingTonnage, 4);
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return value.ToString("C4", culture);
        }

        private static CalcResultLaDisposalCostDataDetail GetHeader()
        {
            return new CalcResultLaDisposalCostDataDetail()
            {
                Name = CommonConstants.Material,
                England = CommonConstants.England,
                Wales = CommonConstants.Wales,
                Scotland = CommonConstants.Scotland,
                NorthernIreland = CommonConstants.NorthernIreland,
                Total = CommonConstants.Total,
                ProducerReportedHouseholdPackagingWasteTonnage = CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage,
                ReportedPublicBinTonnage = CommonConstants.ReportedPublicBinTonnage,
                HouseholdDrinkContainers = CommonConstants.HouseholdDrinkContainers,
                LateReportingTonnage = CommonConstants.LateReportingTonnage,
                ProducerReportedTotalTonnage = CommonConstants.ProducerReportedTotalTonnage,
                DisposalCostPricePerTonne = CommonConstants.DisposalCostPricePerTonne,
                OrderId = 1,
            };
        }

        private static decimal GetDecimalValue(string value)
        {
            var isParseSuccessful = decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal result);
            return isParseSuccessful ? result : 0;
        }

        private static decimal ConvertCurrencyToDecimal(string currency)
        {
            decimal amount;
            decimal.TryParse(currency, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out amount);
            return amount;
        }

        private async Task SetProducerData(CalcResultsRequestDto resultsRequestDto)
        {
            this.producerData = await (from run in this.context.CalculatorRuns
                                       join producerDetail in this.context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                                       join producerMaterial in this.context.ProducerReportedMaterial on producerDetail.Id equals producerMaterial
                                           .ProducerDetailId
                                       join material in this.context.Material on producerMaterial.MaterialId equals material.Id
                                       where run.Id == resultsRequestDto.RunId &&
                                           producerMaterial.PackagingType != null &&
                                           (
                                               producerMaterial.PackagingType == PackagingTypes.Household ||
                                               producerMaterial.PackagingType == PackagingTypes.PublicBin ||
                                               (
                                                   producerMaterial.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                                                   material.Code == MaterialCodes.Glass))
                                       select new ProducerData
                                       {
                                           MaterialName = material.Name,
                                           PackagingType = producerMaterial.PackagingType,
                                           Tonnage = producerMaterial.PackagingTonnage,
                                           ProducerDetail = producerMaterial.ProducerDetail,
                                       }).ToListAsync();
        }
    }
}