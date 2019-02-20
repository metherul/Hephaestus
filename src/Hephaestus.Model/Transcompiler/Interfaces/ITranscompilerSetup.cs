using System.Collections.Generic;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface ITranscompilerSetup : IService
    {
        List<string> GetModOrganizerProfiles();
        void SetModOrganizerExePath(string path);
        void SetModOrganizerProfile(string profile);
        void SetModpackName(string modpackName);
        void SetModpackAuthorName(string authorName);
        void SetModpackSource(string modpackSource);
        void SetModpackVersion(string modpackVersion);
    }
}