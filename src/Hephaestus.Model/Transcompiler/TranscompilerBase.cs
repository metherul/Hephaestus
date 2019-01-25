using Hephaestus.Model.Transcompiler.Interfaces;

namespace Hephaestus.Model.Transcompiler
{
    public class TranscompilerBase : ITranscompilerBase
    {
        public string ModsDirectoryPath { get; set; }
        public string ProfilesDirectoryPath { get; set; }

        public string ChosenProfilePath { get; set; }
    }
}
