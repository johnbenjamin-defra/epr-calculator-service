using EPR.Calculator.Service.Function.Models;
using System.Text;

namespace EPR.Calculator.Service.Function.Exporter.Detail
{
    public interface ICalcResultDetailExporter
    {
        void Export(CalcResultDetail calcResultDetail, StringBuilder stringBuilder);
    }
}