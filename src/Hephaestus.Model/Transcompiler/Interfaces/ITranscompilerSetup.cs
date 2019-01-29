using System.Collections.Generic;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface ITranscompilerSetup : IService
    {
        List<string> GetModOrganizerProfiles();
        void SetModOrganizerExePath(string path);
        void SetModOrganizerProfile(string profile);
    }
}