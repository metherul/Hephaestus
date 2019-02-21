using System.Collections.Generic;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface ITranscompilerBase : IService
    {
        string MODirectory { get; set; }
        string ChosenProfilePath { get; set; }
        string ModsDirectoryPath { get; set; }
        string ProfilesDirectoryPath { get; set; }
        string ModsListFileName { get; set; }
        string ModMetaFileName { get; set; }
        string DownloadsDirectoryPath { get; set; }
        string MOMetaFileName { get; set; }
        string ProfileName { get; set; }
        List<IntermediaryModObject> IntermediaryModObjects { get; set; }

        string ModpackName { get; set; }
        string ModpackAuthorName { get; set; }
        string ModpackSource { get; set; }
        string ModpackVersion { get; set; }
    }
}