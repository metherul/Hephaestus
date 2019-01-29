using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface IArchiveFilesystemSearch : IModel
    {
        string FindArchive(string archive);
    }
}