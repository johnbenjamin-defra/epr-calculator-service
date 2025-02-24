using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        Task<CalcResultCommsCost> Construct(CalcResultsRequestDto resultsRequestDto,
           CalcResultOnePlusFourApportionment apportionment, CalcResult calcResult);
    }
}