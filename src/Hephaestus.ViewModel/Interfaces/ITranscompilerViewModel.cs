using System.Collections.ObjectModel;

namespace Hephaestus.ViewModel.Interfaces
{
    public interface ITranscompilerViewModel : IViewModel
    {
        ObservableCollection<string> ProgressLog { get; set; }
    }
}