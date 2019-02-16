using System.Collections.Generic;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface IModListBuilder : IModel
    {
        List<string> BuildModListAndReturnMissing();

        bool ValidateArchiveAgainstPath(string archiveName, string archivePath);

        bool AddMissingArchive(string archiveName, string archivePath);

        string GetSafeFilename(string filename);
    }
}
