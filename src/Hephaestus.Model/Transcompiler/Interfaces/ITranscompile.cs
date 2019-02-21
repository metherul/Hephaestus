using System;
using System.Threading.Tasks;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface ITranscompile : IModel
    {
        Task Start(IProgress<string> progressLog);
    }
}