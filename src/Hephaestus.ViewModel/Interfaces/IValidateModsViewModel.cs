using System.Collections.ObjectModel;

namespace Hephaestus.ViewModel.Interfaces
{
    public interface IValidateModsViewModel : IViewModel
    {
        ObservableCollection<string> MissingArchives { get; set; }

        void BeginValidation();
    }
}