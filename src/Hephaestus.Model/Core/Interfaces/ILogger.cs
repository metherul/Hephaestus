using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Core.Interfaces
{
    public interface ILogger : IService
    {
        void Write(string message);
    }
}