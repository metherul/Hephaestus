using System;
using System.IO;

namespace Hephaestus.Model.Transcompiler
{
    public class ArchiveModFilePair
    {
        public string ArchiveFile { get; set; }
        public string ModFile { get; set; }

        public ArchiveModFilePair(string parentArchivePath, string parentModPath)
        {
            ArchiveFile = parentArchivePath;
            ModFile = parentModPath;
        }

        public ArchiveModFilePair New(string archiveFilePath, string modFilePath)
        {
            //ModFile = modFilePath.Replace(_parentModPath, "");
            //ArchiveFile = archiveFilePath.Replace(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract"), "");

            return this;
        }
    }
}
