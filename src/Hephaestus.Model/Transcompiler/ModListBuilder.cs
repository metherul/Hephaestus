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

        public List<string> BuildModListAndReturnMissing()
        {
            var modListPath = Path.Combine(_transcompilerBase.ChosenProfilePath, _transcompilerBase.ModsListFileName);
            var missingArchives = new List<string>();

            var modPaths = File.ReadAllLines(modListPath)
                .Where(x => x.StartsWith("+"))
                .Select(x => x.Remove(0, 1))
                .Select(x => Path.Combine(_transcompilerBase.ModsDirectoryPath, x));

            var intermediaryModObjects = new List<IntermediaryModObject>();

            foreach (var mod in modPaths)
            {
                var modIni = Path.Combine(mod, _transcompilerBase.ModMetaFileName);

                if (!File.Exists(modIni))
                {
                    continue;
                }

                var iniParser = new FileIniDataParser();
                var iniData = iniParser.ReadFile(modIni);

                var intermediaryModObject = new IntermediaryModObject()
                {
                    ModPath = mod,
                    IsNexusSource = iniData["General"]["repository"].ToLower() == "nexus"
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

                    intermediaryModObject.ArchivePath = Path.GetFileName(GetSafeFilename(iniData["General"]["installationFile"]));
                    intermediaryModObjects.Add(intermediaryModObject);
                }

                else
                {
                    intermediaryModObject.ArchivePath = archivePath;
                    intermediaryModObjects.Add(intermediaryModObject);
                }
            }

            _transcompilerBase.IntermediaryModObjects = intermediaryModObjects;

            return missingArchives;
        }

        public List<string> AnalyzeDirectory(List<string> missingMods, string directoryPath)
        {
            var directoryFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var file in directoryFiles)
            {
                var matchingMod = missingMods.Where(x => ValidateArchiveAgainstPath(x, file)).ToList();

                if (matchingMod.Any())
                {
                    AddMissingArchive(matchingMod.First(), file);
                    missingMods.Remove(matchingMod.First());
                }
            }

            return missingMods;
        }

        public bool ValidateArchiveAgainstPath(string archiveName, string archivePath)
        {
            var archivePathFileName = Path.GetFileName(archivePath);

            return (archiveName == archivePathFileName);
        }

        public bool AddMissingArchive(string archiveName, string archivePath)
        {
            // At this point, we need to trust that the archive is correct. If not, bubble up errors in later compilation
            var matchingModObject = _transcompilerBase.IntermediaryModObjects.First(x => x.ArchivePath == archiveName);
            _transcompilerBase
                .IntermediaryModObjects[_transcompilerBase.IntermediaryModObjects.IndexOf(matchingModObject)]
                .ArchivePath = archivePath;

            return true;
        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("", filename.Split('\"'));
        }
    }
}
