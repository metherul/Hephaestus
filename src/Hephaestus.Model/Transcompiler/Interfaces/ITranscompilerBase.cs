using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface ITranscompilerBase : IService
    {
        string ChosenProfilePath { get; set; }
        string ModsDirectoryPath { get; set; }
        string ProfilesDirectoryPath { get; set; }
    }
}