using System.Collections.Generic;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface ITranscompilerSetup : IService
    {
        List<string> GetModOrganizerProfiles();
        void SetModOrganizerCsv(string csv);
        void SetModOrganizerExePath(string path);
        void SetModOrganizerProfile();
    }
}