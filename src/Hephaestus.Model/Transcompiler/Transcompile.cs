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

        public async Task Start(IProgress<string> progressLog)
        {
            var intermediaryModObjects = _transcompilerBase.IntermediaryModObjects;

            // Begin the transcompilation process.
            // At this point we can assume that all mods have been validated and require valid meta information.

            foreach (var modObject in intermediaryModObjects)
            {
                progressLog.Report($"[####] {new DirectoryInfo(modObject.ModPath).Name}");
                progressLog.Report($"[INFO] Archive: {new FileInfo(modObject.ArchivePath).Name}");

                _logger.Write($"{modObject.ArchivePath} \n");

                // Calculate an Md5 hash
                progressLog.Report("[INFO] Calculating MD5...");
                modObject.Md5 = _md5.Create(modObject.ArchivePath);
                progressLog.Report("[DONE]");

                // Get ModId and FileId data
                progressLog.Report($"[INFO] Attempting Nexus API request ({_nexusApi.RemainingDailyRequests} daily requests remaining)...");
                if (modObject.IsNexusSource)
                {
                    var md5Response = await _nexusApi.GetModsByMd5(modObject.Md5);

                    if (md5Response == null)
                    {
                        progressLog.Report("[WARN] API request failed. This is generally not an issue.");
                        modObject.TrueArchiveName = Path.GetFileName(modObject.ArchivePath);
                    }

                    else
                    {
                        progressLog.Report("[DONE]");
                        modObject.Author = md5Response.AuthorName;
                        modObject.ModId = md5Response.ModId;
                        modObject.FileId = md5Response.FileId;
                        modObject.TrueArchiveName = md5Response.ArchiveName;
                        modObject.TargetGame = "Skyrim";
                        modObject.Repository = "NexusMods";
                    }
                }

                // Done with data prep.
                // Begin matching pairs of files together (archive, mod).

                progressLog.Report("[INFO] Starting analysis...");

                var archive = new ArchiveFile(modObject.ArchivePath);
                var archiveExtractionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract");

                if (Directory.Exists(archiveExtractionPath))
                {
                    Directory.Delete(archiveExtractionPath, true);
                }

                progressLog.Report($"[INFO] Extracting: {Path.GetFileName(modObject.ArchivePath)}. This may take some time...");
                archive.Extract(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract"));
                progressLog.Report("[DONE] ");

                progressLog.Report("[INFO] Indexing mod and archive files...");
                var archiveFiles = Directory.GetFiles(archiveExtractionPath, "*.*", SearchOption.AllDirectories).Select(x => new FileInfo(x)).ToList();
                var modFiles = Directory.GetFiles(modObject.ModPath, "*.*", SearchOption.AllDirectories);
                progressLog.Report("[DONE]");

                // Search for archive and mod file pairs
                var archiveModPairs = new List<ArchiveModFilePair>();

                progressLog.Report("[INFO] Starting primary mod file analysis...");
                foreach (var modFile in modFiles)
                {
                    if (Path.GetFileName(modFile) == "meta.ini")
                    {
                        continue;
                    }

                    progressLog.Report($"[INFO] Searching for archive file match to: {Path.GetFileName(modFile)}");

                    var modFileInfo = new FileInfo(modFile);
                    var archiveModPair = new ArchiveModFilePair(modObject.ArchivePath, modObject.ModPath);

                    // Attempt to find a match by file length
                    var possibleArchiveMatches = archiveFiles.Where(x => x.Length == modFileInfo.Length).ToList();

                    if (possibleArchiveMatches.Any() && possibleArchiveMatches.Count() == 1)
                    {
                        progressLog.Report($"[DONE] Match found: {possibleArchiveMatches.First().Name}");

                        archiveModPairs.Add(archiveModPair.New(possibleArchiveMatches.First().FullName, modFileInfo.FullName));
                        continue;
                    }

                    progressLog.Report("[INFO] Using advanced match algorithm...");
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
                        progressLog.Report($"[INFO] Checking possible match: {possibleArchiveMatch.Name}");

                        if (AreFilesEqual(possibleArchiveMatch, modFileInfo))
                        {
                            progressLog.Report($"[DONE] Match found: {possibleArchiveMatch.Name}");

                            archiveModPairs.Add(archiveModPair.New(possibleArchiveMatch.FullName, modFileInfo.FullName));
                            break;
                        }
                    }

                    if (!archiveModPairs.Contains(archiveModPair))
                    {
                        progressLog.Report($"[WARN] {Path.GetFileName(modFile)} has no valid match.");
                        progressLog.Report("[DONE]");
                        _logger.Write($"{modFile} had no valid match. \n");
                    }
                }

                modObject.ArchiveModFilePairs = archiveModPairs;

                progressLog.Report("[INFO] Removing extracted files...");
                Directory.Delete(archiveExtractionPath, true);
                progressLog.Report("[DONE]");
            }

            // Write modpack to file

            progressLog.Report("[INFO] Exporting to modpack...");
            await _modpackExport.ExportModpack();
            progressLog.Report("[DONE]");

            progressLog.Report("");
            progressLog.Report("[INFO] OPERATION COMPLETED");
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
        public FileInfo CurrentModFile { get; set; }
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
