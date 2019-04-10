using Autofac;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.Model.Nexus.Interfaces;
using Hephaestus.Model.Transcompiler.Interfaces;
using IniParser;
using Newtonsoft.Json;
using SevenZipExtractor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hephaestus.Model.Transcompiler
{
    public class Transcompile2 : ITranscompile
    {
        private readonly ITranscompilerBase _transcompilerBase;
        private readonly IMd5 _md5;
        private readonly INexusApi _nexusApi;
        private readonly IModpackExport _modpackExport;
        private readonly ILogger _logger;

        public Transcompile2(IComponentContext components)
        {
            _transcompilerBase = components.Resolve<ITranscompilerBase>();
            _nexusApi = components.Resolve<INexusApi>();
            _modpackExport = components.Resolve<IModpackExport>();
            _logger = components.Resolve<ILogger>();
        }

        public async Task Start(IProgress<string> progressLog)
        {
            var stopwatch = new Stopwatch();
            var download_metadata = await GetDownloadMetadata(progressLog);
            Update(progressLog, "[META] Retrieved metadata for", download_metadata.Count(), "archives");
            stopwatch.Start();

            Parallel.ForEach(_transcompilerBase.IntermediaryModObjects,
             mod =>
            {
                CompileMod(progressLog, mod, download_metadata);
            });

            progressLog.Report("[INFO] Exporting to modpack...");
            await _modpackExport.ExportModpack();
            progressLog.Report("[DONE]");

            progressLog.Report("");
            stopwatch.Stop();
            Update(progressLog, "[INFO] OPERATION COMPLETED - ", stopwatch.Elapsed.TotalSeconds.ToString(), "sec");


            return;
        }

        private void CompileMod(IProgress<string> progressLog, IntermediaryModObject mod, IEnumerable<ArchiveContents> download_metadata)
        {
            var mod_name = Path.GetFileName(mod.ModPath);
            Update(progressLog, "[MOD] Compiling", mod_name);
            var matching_archive = download_metadata.Where(md => md.DiskName == Path.GetFileName(mod.ArchivePath)).FirstOrDefault();
            if (matching_archive == null)
            {
                Update(progressLog, "[MOD] No match for", mod_name);
                return;

            }
            mod.Author = matching_archive.Author;
            mod.IsNexusSource = matching_archive.Repository == "Nexus";
            mod.TargetGame = matching_archive.TargetGame;
            mod.Md5 = matching_archive.MD5;
            mod.ModId = matching_archive.NexusModId;
            mod.FileId = matching_archive.NexusFileId;
            mod.NexusFileName = matching_archive.NexusFileName;
            mod.TrueArchiveName = matching_archive.DiskName;
            mod.Version = matching_archive.Version;

            var indexed = matching_archive.Contents.GroupBy(k => k.SHA256).ToDictionary(k => k.Key);
            mod.ArchiveModFilePairs = new List<ArchiveModFilePair>();

            Parallel.ForEach(Directory.EnumerateFiles(mod.ModPath, "*", SearchOption.AllDirectories),
                file =>
            {
                if (Path.GetFileName(file) == "meta.ini") return;
                var shortened_name = file.Substring(mod.ModPath.Length);
                var hash = Extensions.SHA256(file);


                if (indexed.TryGetValue(hash, out var matches))
                {
                    //if (matches.Count() > 0)
                    //Update(progressLog, "[MOD] WARNING: multiple matches found for", shortened_name);
                    mod.ArchiveModFilePairs.Add(new ArchiveModFilePair("\\" + matches.First().FileName, shortened_name));
                }
                else
                {
                    //Update(progressLog, "[MOD] No Match found for", shortened_name);
                }

            });

        }

        private static ISet<string> SUPPORTED_ARCHIVES = new HashSet<string>() { ".zip", ".7z", ".7zip", ".rar" };
        private static string METADATA_EXTENSION = ".archive_meta";

        private void Update(IProgress<string> progress, params object[] vals)
        {
            var msg = string.Join(" ", vals.Select(v => v.ToString()));
            _logger.Write(msg);
            progress.Report(msg);
        }

        private async Task<IEnumerable<ArchiveContents>> GetDownloadMetadata(IProgress<string> progress)
        {
            var tasks = Directory.EnumerateFiles(_transcompilerBase.DownloadsDirectoryPath, "*.*").AsParallel()
                .Select(async archive => {
                    if (!SUPPORTED_ARCHIVES.Contains(Path.GetExtension(archive).ToLower())) return;
                    var meta_data_path = Path.Combine(_transcompilerBase.DownloadsDirectoryPath, archive) + METADATA_EXTENSION;

                    if (File.Exists(meta_data_path)) return;
                    Update(progress, "[META] Generating Metadata for: ", Path.GetFileName(archive));
                    

                    using (var archive_file = new ArchiveFile(archive))
                    {
                        var streams = new Dictionary<string, HashingStream>();
                        archive_file.Extract(e =>
                        {
                            if (e.IsFolder) return null;
                            if (streams.ContainsKey(e.FileName))
                                return streams[e.FileName];

                            var stream = new HashingStream(e.FileName);
                            streams.Add(e.FileName, stream);
                            return stream;
                        });

                        var contents = from stream in streams.Values
                                       select new ArchiveEntry
                                       {
                                           FileName = stream.Filename,
                                           MD5 = stream.MD5Hash,
                                           SHA256 = stream.SHA256Hash,
                                           Size = stream.Size.ToString()
                                       };

                        var ac = new ArchiveContents();


                            using (var file = File.OpenRead(archive))
                            {
                                var hasher = new MD5CryptoServiceProvider();
                                hasher.ComputeHash(file);
                                ac.MD5 = HashingStream.ToHex(hasher.Hash);
                            }

                            using (var file = File.OpenRead(archive))
                            {
                                var hasher = new SHA256CryptoServiceProvider();
                                hasher.ComputeHash(file);
                                ac.SHA256 = HashingStream.ToHex(hasher.Hash);
                            }
                            ac.DiskName = Path.GetFileName(archive);
                            ac.FileSize = (new FileInfo(archive)).Length.ToString();
                            ac.Contents = contents.ToArray();

                        var meta_path = archive + ".meta";
                        if (File.Exists(meta_path))
                        {
                            var meta_ini = (new FileIniDataParser()).ReadFile(meta_path);
                            var general = meta_ini["General"];

                            ac.NexusModId = general["modID"];
                            ac.NexusFileId = general["fileID"];
                            ac.TargetGame = general["gameName"];
                            ac.ModName = general["modName"];
                            ac.Version = general["version"];
                            ac.Repository = general["repository"];
                        }

                        var result = await _nexusApi.GetModsByMd5(new IntermediaryModObject
                        {
                            Md5 = ac.MD5,
                            TrueArchiveName = ac.DiskName,
                            TargetGame = ac.TargetGame
                        });

                        if (result == null)
                            return;

                        ac.Author = result.AuthorName;
                        ac.NexusModId = result.ModId;
                        ac.NexusFileName = result.NexusFileName;
                        ac.NexusFileId = result.FileId;
                        ac.Version = result.Version;
                        ac.FileSize = (new FileInfo(archive)).Length.ToString();

                        await WriteJSON(meta_data_path, ac);
                        Update(progress, "[META] Finished", Path.GetFileName(archive));
                    }

                });

            await Task.WhenAll(tasks);

            ConcurrentStack<ArchiveContents> loaded = new ConcurrentStack<ArchiveContents>();


            var result_tasks = Directory.EnumerateFiles(_transcompilerBase.DownloadsDirectoryPath, "*" + METADATA_EXTENSION)
                                    .AsParallel()
                                    .Select(async file =>
                                    {
                                        using (var r = new StreamReader(File.OpenRead(file)))
                                        {
                                            return JsonConvert.DeserializeObject<ArchiveContents>(await r.ReadToEndAsync());
                                        }
                                    });

            await Task.WhenAll(result_tasks);

            var results = result_tasks.Select(t => t.Result).ToList();

            return results;
        }

        private async Task<bool> WriteJSON(string meta_data_path, ArchiveContents ac)
        { 
            using (var file = new StreamWriter(File.OpenWrite(meta_data_path)))
            {
                await file.WriteAsync(JsonConvert.SerializeObject(ac, new JsonSerializerSettings() {Formatting = Formatting.Indented } ));
            }
            return true;
        }
    }
}
