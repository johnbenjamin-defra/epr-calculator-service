namespace EPR.Calculator.Service.Function.Exporter
{
    using System;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.Extensions.Primitives;

    public interface ILateReportingExporter
    {
        string Export(CalcResultLateReportingTonnage? calcResultLateReportingData);
    }

    /// <summary>
    /// Exports the Late Reporting Tonnage data to a string to be added to the results file.
    /// </summary>
    /// <param name="calcResultLateReportingData">The data to export.</param>
    public class LateReportingExporter : ILateReportingExporter
    {
        public string Export(CalcResultLateReportingTonnage? calcResultLateReportingData)
        {
            var csvContent = new StringBuilder();
            if (calcResultLateReportingData is null)
            {
                return string.Empty;
            }

            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData(calcResultLateReportingData.Name));
            csvContent.Append(CsvSanitiser.SanitiseData($"{calcResultLateReportingData.MaterialHeading}"));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLateReportingData.TonnageHeading));
            csvContent.AppendLine();

            foreach (var lateReportingData in calcResultLateReportingData.CalcResultLateReportingTonnageDetails)
            {
                csvContent.AppendJoin(
                    string.Empty,
                    new string[]
                    {
                        lateReportingData.Name,
                        lateReportingData.TotalLateReportingTonnage.ToString("0.000"),
                    }.Select(s => CsvSanitiser.SanitiseData(s)));
                csvContent.AppendLine();
            }

            return csvContent.ToString();
        }
    }
}
