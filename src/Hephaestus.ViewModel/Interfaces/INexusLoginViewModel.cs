using GalaSoft.MvvmLight.Command;

namespace Hephaestus.ViewModel.Interfaces
{
    public interface INexusLoginViewModel : IViewModel
    {
        RelayCommand LoginToNexusCommand { get; }
    }
}