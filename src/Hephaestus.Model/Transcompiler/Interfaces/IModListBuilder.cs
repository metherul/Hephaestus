using System.Collections.Generic;
using Hephaestus.Model.Interfaces;

namespace Hephaestus.Model.Transcompiler.Interfaces
{
    public interface IModListBuilder : IModel
    {
        (string, List<string>) BuildModListAndReturnMissing();
    }
}
