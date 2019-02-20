using System;
using System.Reflection;
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

        public RelayCommand<string> LoginToNexusCommand => new RelayCommand<string>(LoginToNexus);

        public bool IsLoggingIn { get; set; }
        public bool HasSavedApiKey { get; set; }

        public NexusLoginViewModel(IComponentContext components)
        {
            _nexusApi = components.Resolve<INexusApi>();
            _viewIndexController = components.Resolve<IViewIndexController>();

            HasSavedApiKey = Properties.Settings.Default.ApiKey != string.Empty;
        }

        private void LoginToNexus(string useSavedKey = "false")
        {
            var test = Properties.Settings.Default.ApiKey;

            IsLoggingIn = true;

            _nexusApi.HasLoggedInEvent += () =>
            {
                IsLoggingIn = false;

                Properties.Settings.Default.ApiKey = _nexusApi.ApiKey();
                Properties.Settings.Default.Save();

                _viewIndexController.SetCurrentViewIndex(ViewIndex.MainPage);
            };

            if (useSavedKey == "true")
            {
                _nexusApi.New(GameName.Skyrim, Properties.Settings.Default.ApiKey);
            }

            else
            {
                _nexusApi.New(GameName.Skyrim);
            }
        }
    }
}
