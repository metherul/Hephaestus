using System.Collections.Generic;
using Hephaestus.Model.Transcompiler.Interfaces;

namespace Hephaestus.Model.Transcompiler
{
    public class IntermediaryModObject
    {
        public List<ArchiveModFilePair> ArchiveModFilePairs { get; set; }

        public string ModId { get; set; }
        public string FileId { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string ModPath { get; set; }
        public string ArchivePath { get; set; }
        public string TrueArchiveName { get; set; }

        public string TargetGame { get; set; }
        public string Repository { get; set; }

        public string Md5 { get; set; }

        public bool IsNexusSource { get; set; }
    }
}
