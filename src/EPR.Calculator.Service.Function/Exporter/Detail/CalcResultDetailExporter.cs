using EPR.Calculator.API.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.Detail
{
    public class CalcResultDetailexporter : ICalcResultDetailExporter
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

        public void Export(CalcResultDetail calcResultDetail, StringBuilder csvContent)
        {
            AppendCsvLine(csvContent, RunName, calcResultDetail.RunName);
            AppendCsvLine(csvContent, RunId, calcResultDetail.RunId.ToString());
            AppendCsvLine(csvContent, RunDate, calcResultDetail.RunDate.ToString(CalculationResults.DateFormat));
            AppendCsvLine(csvContent, Runby, calcResultDetail.RunBy);
            AppendCsvLine(csvContent, FinancialYear, calcResultDetail.FinancialYear);
            AppendRpdFileInfo(csvContent, RPDFileORG, RPDFilePOM, calcResultDetail.RpdFileORG, calcResultDetail.RpdFilePOM);
            AppendFileInfo(csvContent, LapcapFile, calcResultDetail.LapcapFile);
            AppendFileInfo(csvContent, ParametersFile, calcResultDetail.ParametersFile);
            AppendFileInfo(csvContent, CountryApportionmentFile, calcResultDetail.CountryApportionmentFile);
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
    }
}