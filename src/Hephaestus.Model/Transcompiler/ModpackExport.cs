using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Modpack.ModpackBase;
using Hephaestus.Model.Transcompiler.Interfaces;
using Newtonsoft.Json;

namespace Hephaestus.Model.Transcompiler
{
    public class ModpackExport
    {
        private readonly ITranscompilerBase _transcompilerBase;

        public ModpackExport(IComponentContext components)
        {
            _transcompilerBase = components.Resolve<ITranscompilerBase>();
        }

        public async Task ExportModpack()
        {
            var intermediaryModObjects = _transcompilerBase.IntermediaryModObjects;
            var modpackDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                Path.GetDirectoryName(_transcompilerBase.ChosenProfilePath));

            if (Directory.Exists(modpackDirectory))
            {
                Directory.Delete(modpackDirectory, true);
            }

            Directory.CreateDirectory(modpackDirectory);

            // Create the modpack header
            var modpackHeader = new Header()
            {
                Author = "",
                ModInstallFolders = new List<string>() {"mods"},
                Name = "Some Modpack",
                SourceUrl = "",
                TargetGame = "Skyrim",
                Version = "1.0"
            };

            // Write to modpack directory
            File.WriteAllText(Path.Combine(modpackDirectory, "header.json"),
                JsonConvert.SerializeObject(modpackHeader));

            // Create the "mods" subdirectory
            var modsDirectory = Path.Combine(modpackDirectory, "mods");
            Directory.CreateDirectory(modsDirectory);

            foreach (var modObject in intermediaryModObjects)
            {
                var mod = new Mod()
                {
                    ModName = Path.GetDirectoryName(modObject.ModPath),
                    FileName = Path.GetFileName(modObject.ArchivePath),
                    FileSize = new FileInfo(modObject.ArchivePath).Length.ToString(),
                    Md5 = modObject.Md5.ToLower(),
                    ModId = modObject.ModId,
                    FileId = modObject.FileId
                };

                var installationParameters = modObject.ArchiveModFilePairs.Select(x => new InstallParameter()
                {
                    SourceLocation = x.ArchiveFile,
                    TargetLocation = x.ModFile
                }).ToList();

                mod.InstallParameters = installationParameters;

                // Write to file
                File.WriteAllText(Path.Combine(modsDirectory, mod.ModName), JsonConvert.SerializeObject(mod));
            }
        }
    }
}
