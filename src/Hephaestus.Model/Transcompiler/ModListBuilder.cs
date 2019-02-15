using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Hephaestus.Model.Transcompiler.Interfaces;
using IniParser;

namespace Hephaestus.Model.Transcompiler
{
    public class ModListBuilder : IModListBuilder
    {
        private readonly ITranscompilerBase _transcompilerBase;
        private readonly IArchiveFilesystemSearch _archiveFilesystemSearch;

        public ModListBuilder(IComponentContext components)
        {
            _transcompilerBase = components.Resolve<ITranscompilerBase>();
            _archiveFilesystemSearch = components.Resolve<IArchiveFilesystemSearch>();
        }

        public (string, List<string>) BuildModListAndReturnMissing()
        {
            var modListPath = Path.Combine(_transcompilerBase.ChosenProfilePath, _transcompilerBase.ModsListFileName);
            var missingArchives = new List<string>();

            if (!File.Exists(modListPath))
            {
                return ($@"{modListPath} not found. This is an issue with your currently selected MO profile.", null);
            }

            var modPaths = File.ReadAllLines(modListPath)
                .Where(x => x.StartsWith("+"))
                .Select(x => x.Remove(0, 1))
                .Select(x => Path.Combine(_transcompilerBase.ModsDirectoryPath, x));

            var intermediaryModObjects = new List<IntermediaryModObject>();

            foreach (var mod in modPaths)
            {
                var modIni = Path.Combine(mod, _transcompilerBase.ModMetaFileName);

                var iniParser = new FileIniDataParser();
                var iniData = iniParser.ReadFile(modIni);

                var intermediaryModObject = new IntermediaryModObject()
                {
                    ModPath = mod
                };

                var archivePath = GetSafeFilename(iniData["General"]["installationFile"]);

                // If the archive doesn't exist, initialize advanced archive search.
                if (!File.Exists(archivePath))
                {
                    archivePath = _archiveFilesystemSearch.FindArchive(archivePath);
                }

                if (archivePath == string.Empty)
                {
                    missingArchives.Add(Path.GetFileName(GetSafeFilename(iniData["General"]["installationFile"])));
                }

                else
                {
                    intermediaryModObject.ArchivePath = archivePath;
                    intermediaryModObjects.Add(intermediaryModObject);
                }
            }

            _transcompilerBase.IntermediaryModObjects = intermediaryModObjects;

            return (string.Empty, missingArchives);
        }

        public (bool, string) ValidateArchiveAgainstPath(string archiveName, string archivePath)
        {
            var archivePathFileName = Path.GetFileName(archivePath);

            return (archiveName == archivePathFileName) ?  (true, "") :  (false, "Archive name and selected path do not match.");
        }

        public bool AddMissingArchiveMod(string archiveName, string archivePath)
        {
            // At this point, we need to trust that the archive is correct. If not, bubble up errors in later compilation


            return false;
        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("", filename.Split('\"'));
        }
    }
}
