using System.Collections.ObjectModel;
using Hephaestus.Model.Transcompiler;

namespace Hephaestus.ViewModel.Interfaces
{
    public interface ITranscompilerViewModel : IViewModel
    {
        ObservableCollection<IntermediaryModObject> ProgressLog { get; set; }
    }
}