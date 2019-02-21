using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.Model.Nexus.Interfaces;
using Hephaestus.Model.Transcompiler.Interfaces;
using SevenZipExtractor;

namespace Hephaestus.Model.Transcompiler
{
    public class Transcompile : ITranscompile
    {
        private readonly ITranscompilerBase _transcompilerBase;
        private readonly IMd5 _md5;
        private readonly INexusApi _nexusApi;
        private readonly IModpackExport _modpackExport;
        private readonly ILogger _logger;

        public Transcompile(IComponentContext components)
        {
            _transcompilerBase = components.Resolve<ITranscompilerBase>();
            _md5 = components.Resolve<IMd5>();
            _nexusApi = components.Resolve<INexusApi>();
            _modpackExport = components.Resolve<IModpackExport>();
            _logger = components.Resolve<ILogger>();
        }

        public async Task Start(IProgress<TranscompileProgress> progressLog)
        {
            var intermediaryModObjects = _transcompilerBase.IntermediaryModObjects;

            // Begin the transcompilation process.
            // At this point we can assume that all mods have been validated and require valid meta information.

            foreach (var modObject in intermediaryModObjects)
            {
                _logger.Write($"{modObject.ArchivePath} \n");

                // Calculate an Md5 hash
                modObject.Md5 = _md5.Create(modObject.ArchivePath);

                // Get ModId and FileId data
                if (modObject.IsNexusSource)
                {
                    var md5Response = await _nexusApi.GetModsByMd5(modObject.Md5);

                    if (md5Response == null)
                    {
                        modObject.TrueArchiveName = Path.GetFileName(modObject.ArchivePath);
                    }

                    else
                    {
                        modObject.Author = md5Response.AuthorName;
                        modObject.ModId = md5Response.ModId;
                        modObject.FileId = md5Response.FileId;
                        modObject.TrueArchiveName = md5Response.ArchiveName;
                    }
                }

                // Done with data prep.
                // Begin matching pairs of files together (archive, mod).

                var archive = new ArchiveFile(modObject.ArchivePath);
                var archiveExtractionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract");

                archive.Extract(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract"));

                var archiveFiles = Directory.GetFiles(archiveExtractionPath, "*.*", SearchOption.AllDirectories).Select(x => new FileInfo(x)).ToList();
                var modFiles = Directory.GetFiles(modObject.ModPath, "*.*", SearchOption.AllDirectories);

                // Search for archive and mod file pairs
                var archiveModPairs = new List<ArchiveModFilePair>();

                foreach (var modFile in modFiles)
                {
                    var modFileInfo = new FileInfo(modFile);
                    var archiveModPair = new ArchiveModFilePair(modObject.ArchivePath, modObject.ModPath);

                    // Attempt to find a match by file length
                    var possibleArchiveMatches = archiveFiles.Where(x => x.Length == modFileInfo.Length).ToList();

                    if (possibleArchiveMatches.Any() && possibleArchiveMatches.Count() == 1)
                    {
                        archiveModPairs.Add(archiveModPair.New(possibleArchiveMatches.First().FullName, modFileInfo.FullName));
                        continue;
                    }

                    // More than one match. Fall back to advanced identification.
                    if (possibleArchiveMatches.Count(x => x.Name == modFileInfo.Name) == 1)
                    {
                        var index = possibleArchiveMatches.IndexOf(possibleArchiveMatches.First(x => x.Name == modFileInfo.Name));
                        var item = possibleArchiveMatches[index];

                        possibleArchiveMatches.RemoveAt(index);
                        possibleArchiveMatches.Insert(0, item);
                    }

                    foreach (var possibleArchiveMatch in possibleArchiveMatches)
                    {
                        if (AreFilesEqual(possibleArchiveMatch, modFileInfo))
                        {
                            archiveModPairs.Add(archiveModPair.New(possibleArchiveMatch.FullName, modFileInfo.FullName));
                            break;
                        }
                    }

                    if (!archiveModPairs.Contains(archiveModPair))
                    {
                        _logger.Write($"{modFile} had no valid match. \n");
                    }
                }

                modObject.ArchiveModFilePairs = archiveModPairs;

                Directory.Delete(archiveExtractionPath, true);
            }

            // Write modpack to file

            await _modpackExport.ExportModpack();
        }

        private bool AreFilesEqual(FileInfo archiveFile, FileInfo modFile)
        {
            var bytesToRead = 8;
            var iterations = (int)Math.Ceiling((double)modFile.Length / bytesToRead);

            using (var modFileStream = modFile.OpenRead())
            {
                var two = new byte[bytesToRead];
                var one = new byte[bytesToRead];

                using (var archiveFileStream = archiveFile.OpenRead())

                for (var i = 0; i < iterations; i++)
                {
                    modFileStream.Read(one, 0, bytesToRead);
                    archiveFileStream.Read(two, 0, bytesToRead);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class TranscompileProgress
    {
        public int TotalModCount { get; set; }
        public int CurrentModCount { get; set; }
        public IntermediaryModObject CurrentModObject { get; set; }
        public CurrentTranscompileStep CurrentTranscompileStep { get; set; }
    }

    public enum CurrentTranscompileStep
    {
        NexusApi,
        Extraction,
        Indexing,
        Matching
    }
}
