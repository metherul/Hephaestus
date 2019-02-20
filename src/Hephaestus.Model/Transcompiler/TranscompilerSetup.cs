using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hephaestus.Model.Transcompiler.Interfaces;

namespace Hephaestus.Model.Transcompiler
{
    public class TranscompilerSetup : ITranscompilerSetup
    {
        private readonly ITranscompilerBase _transcompilerBase;

        public TranscompilerSetup(ITranscompilerBase transcompilerBase)
        {
            _transcompilerBase = transcompilerBase;
        }

        public void SetModOrganizerExePath(string path)
        {
            var pathDirectory = Path.GetDirectoryName(path);

            _transcompilerBase.MODirectory = pathDirectory;
            _transcompilerBase.ModsDirectoryPath = Path.Combine(pathDirectory, "mods");
            _transcompilerBase.ProfilesDirectoryPath = Path.Combine(pathDirectory, "profiles");
            _transcompilerBase.DownloadsDirectoryPath = Path.Combine(pathDirectory, "downloads");
        }

        public List<string> GetModOrganizerProfiles()
        {
            return Directory.GetDirectories(_transcompilerBase.ProfilesDirectoryPath)
                .Select(x => new DirectoryInfo(x).Name)
                .ToList();
        }

        public void SetModOrganizerProfile(string profileName)
        {
            _transcompilerBase.ChosenProfilePath =
                Path.Combine(_transcompilerBase.ProfilesDirectoryPath, profileName);
        }

        public void SetModpackName(string modpackName)
        {
            _transcompilerBase.ModpackName = modpackName;
        }

        public void SetModpackAuthorName(string authorName)
        {
            _transcompilerBase.ModpackAuthorName = authorName;
        }

        public void SetModpackSource(string modpackSource)
        {
            _transcompilerBase.ModpackSource = modpackSource;
        }

        public void SetModpackVersion(string version)
        {
            _transcompilerBase.ModpackVersion = version;
        }
    }
}
