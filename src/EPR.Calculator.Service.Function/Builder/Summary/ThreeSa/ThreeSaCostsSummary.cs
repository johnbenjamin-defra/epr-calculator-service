using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using System.Collections.Generic;
using System.Linq;

namespace EPR.Calculator.Service.Function.Builder.Summary.ThreeSA
{
    public static class ThreeSaCostsSummary
    {
        public static readonly int ColumnIndex = 245;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.SaOperatingCostsWithoutBadDebtProvisionTitleSection3}", ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.BadDebtProvisionTitleSection3}", ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.SaOperatingCostsWithBadDebtProvisionTitleSection3}", ColumnIndex = ColumnIndex + 2 }
            ];
        }

        public static decimal GetThreeSaCostsWithoutBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.SaOperatingCost.OrderByDescending(t => t.OrderId).First().TotalValue;
        }

        public static decimal GetBadDebtProvisionSection3(CalcResult calcResult)
        {
            return GetThreeSaCostsWithoutBadDebtProvision(calcResult) * calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        }

        public static decimal GetThreeSaCostsWithBadDebtProvision(CalcResult calcResult)
        {
            return GetThreeSaCostsWithoutBadDebtProvision(calcResult) + GetBadDebtProvisionSection3(calcResult);
        }


        public static decimal GetSetUpBadDebtProvision(CalcResult calcResult)
        {
            return calcResult.CalcResultParameterOtherCost.BadDebtValue;
        }
    }
}