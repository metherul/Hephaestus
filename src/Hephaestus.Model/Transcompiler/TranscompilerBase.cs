using System.Collections.Generic;
using Hephaestus.Model.Transcompiler.Interfaces;

namespace Hephaestus.Model.Transcompiler
{
    public class TranscompilerBase : ITranscompilerBase
    {
        public string MODirectory { get; set; }
        public string ModsDirectoryPath { get; set; }
        public string ProfilesDirectoryPath { get; set; }
        public string DownloadsDirectoryPath { get; set; }
        public string ProfileName { get; set; }

        public string ModpackName { get; set; }
        public string ModpackAuthorName { get; set; }
        public string ModpackSource { get; set; }
        public string ModpackVersion { get; set; }

        public string ChosenProfilePath { get; set; }

        public string ModsListFileName { get; set; } = "modlist.txt";
        public string ModMetaFileName { get; set; } = "meta.ini";
        public string MOMetaFileName { get; set; } = "ModOrganizer.ini";

        public List<IntermediaryModObject> IntermediaryModObjects { get; set; }
    }
}
