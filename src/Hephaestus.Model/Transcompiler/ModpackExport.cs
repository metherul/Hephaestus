using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Modpack.ModpackBase;
using Hephaestus.Model.Transcompiler.Interfaces;
using Ionic.Zip;
using Newtonsoft.Json;

namespace Hephaestus.Model.Transcompiler
{
    public class ModpackExport : IModpackExport
    {
        private readonly ITranscompilerBase _transcompilerBase;

        public ModpackExport(IComponentContext components)
        {
            _transcompilerBase = components.Resolve<ITranscompilerBase>();
        }

        public async Task ExportModpack()
        {
            var intermediaryModObjects = _transcompilerBase.IntermediaryModObjects;

            // Create the modpack header
            var modpackHeader = new Header()
            {
                Author = (_transcompilerBase.ModpackAuthorName == "") ? "Anon" : _transcompilerBase.ModpackAuthorName,
                ModInstallFolders = new List<string>(){ _transcompilerBase.ProfileName },
                Name = (_transcompilerBase.ModpackName == "") ? "Unknown" : _transcompilerBase.ModpackName,
                SourceUrl = (_transcompilerBase.ModpackSource),
                Version = (_transcompilerBase.ModpackVersion == "") ? "1.0" : _transcompilerBase.ModpackVersion,
            };

            var modpackDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, modpackHeader.Name);

            if (Directory.Exists(modpackDirectory))
            {
                Directory.Delete(modpackDirectory, true);
            }

            Directory.CreateDirectory(modpackDirectory);

            // Write to modpack directory
            var headerPath = Path.Combine(modpackDirectory, "modpack.json");

            if (!File.Exists(headerPath))
            {
                File.Create(headerPath).Close();
            }

            File.WriteAllText(headerPath, JsonConvert.SerializeObject(modpackHeader, Formatting.Indented));

            // Create the "mods" subdirectory
            var modsDirectory = Path.Combine(modpackDirectory, modpackHeader.ModInstallFolders.First());
            Directory.CreateDirectory(modsDirectory);

            foreach (var modObject in intermediaryModObjects)
            {
                var mod = new Mod()
                {
                    ModName = new DirectoryInfo(modObject.ModPath).Name,
                    FileName = modObject.TrueArchiveName,
                    FileSize = new FileInfo(modObject.ArchivePath).Length.ToString(),
                    Md5 = modObject.Md5.ToLower(),
                    ModId = modObject.ModId,
                    FileId = modObject.FileId,
                    TargetGame = modObject.TargetGame,
                    Repository = modObject.Repository
                };

                var installationParameters = modObject.ArchiveModFilePairs.Select(x => new InstallParameter()
                {
                    SourceLocation = x.ArchiveFile,
                    TargetLocation = x.ModFile
                }).ToList();

                mod.InstallParameters = installationParameters;

                var modPath = Path.Combine(modsDirectory, mod.ModName + ".json");

                if (!File.Exists(modPath))
                {
                    File.Create(modPath).Close();
                }

                // Write to file
                File.WriteAllText(modPath, JsonConvert.SerializeObject(mod, Formatting.Indented));
            }

            // Move plugin and modlist information from MO2
            File.Copy(Path.Combine(_transcompilerBase.ChosenProfilePath, "plugins.txt"), Path.Combine(modpackDirectory, "plugins.txt"));
            File.Copy(Path.Combine(_transcompilerBase.ChosenProfilePath, "modlist.txt"), Path.Combine(modpackDirectory, "modlist.txt"));

            if (File.Exists(modpackDirectory + ".auto"))
            {
                File.Delete(modpackDirectory + ".auto");
            }

            // Move to .auto modpack file
            using (var zip = new ZipFile())
            {
                foreach (var file in Directory.GetFiles(modpackDirectory, "*.*", SearchOption.AllDirectories))
                {
                    var fileDirectory = Path.GetDirectoryName(file).Replace(modpackDirectory, "");

                    zip.AddFile(file, fileDirectory);
                }

                zip.Save(modpackDirectory + ".auto");
            }

            Directory.Delete(modpackDirectory, true);
        }
    }
}
