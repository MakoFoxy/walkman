using System.Threading.Tasks;

namespace Player.Services.Report.Abstractions
{
    public interface IReportGenerator<in TModel> where TModel : IReportModel
    {
        Task<GeneratorResult> Generate(TModel model);
    }
}