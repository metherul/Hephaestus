using Hephaestus.Model.Interfaces;
using System.Runtime.CompilerServices;

namespace Hephaestus.Model.Core.Interfaces
{
    public interface ILogger : IService
    {
        void Write(string message, [CallerMemberName] string callerName = "");
    }
}