using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hephaestus.Model.Transcompiler
{
    class ArchiveContents
    {
        [JsonProperty("disk_name")]
        public string DiskName { get; set;}

        [JsonProperty("md5")]
        public string MD5 { get; set; }

        [JsonProperty("sha256")]
        public string SHA256 { get; set; }

        [JsonProperty("nexus_mod_id")]
        public string NexusModId { get; set; }

        [JsonProperty("nexus_file_id")]
        public string NexusFileId { get; set; }

        [JsonProperty("nexus_file_name")]
        public string NexusFileName { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("repository")]
        public string Repository { get; set; }

        [JsonProperty("target_game")]
        public string TargetGame { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("mod_name")]
        public string ModName { get; set; }

        [JsonProperty("file_size")]
        public string FileSize { get; set; }

        [JsonProperty("contents")]
        public ArchiveEntry[] Contents;

    }

    class ArchiveEntry
    {
        [JsonProperty("md5")]
        public string MD5 { get; set; }

        [JsonProperty("sha256")]
        public string SHA256 { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("size")] 
        public string Size { get; set; }
    }
}
