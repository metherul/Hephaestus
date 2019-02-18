using Autofac;
using GalaSoft.MvvmLight.Command;
using Hephaestus.Model.Core;
using Hephaestus.Model.Nexus.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class NexusLoginViewModel : ViewModelBase, INexusLoginViewModel
    {
        private readonly INexusApi _nexusApi;
        private readonly IViewIndexController _viewIndexController;

        public RelayCommand LoginToNexusCommand => new RelayCommand(LoginToNexus);

        public bool IsLoggingIn { get; set; }

        public NexusLoginViewModel(IComponentContext components)
        {
            _nexusApi = components.Resolve<INexusApi>();
            _viewIndexController = components.Resolve<IViewIndexController>();
        }

        private void LoginToNexus()
        {
            IsLoggingIn = true;

            _nexusApi.HasLoggedInEvent += () =>
            {
                IsLoggingIn = false;
                _viewIndexController.SetCurrentViewIndex(ViewIndex.MainPage);
            };

            _nexusApi.New(GameName.Skyrim);
        }
    }
}
