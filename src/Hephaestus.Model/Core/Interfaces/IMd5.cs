using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Core.Interfaces
{
    public interface IMd5 : IModel
    {
        string Create(string filePath);
    }
}