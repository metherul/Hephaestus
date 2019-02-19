using System.Threading.Tasks;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface IModpackExport : IModel
    {
        Task ExportModpack();
    }
}