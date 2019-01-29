using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using GalaSoft.MvvmLight.Command;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class SetupModpackViewModel : ViewModelBase, ISetupModpackViewModel
    {
        private readonly ITranscompilerSetup _transcompilerSetup;
        private readonly IModListBuilder _modListBuilder;

        public RelayCommand OpenDirectoryBrowserCommand { get => new RelayCommand(OpenDirectoryBrowser); }
        public RelayCommand<string> ContextMenuSelectionChangedCommand { get => new RelayCommand<string>(ContextMenuSelectionChanged); }
        public RelayCommand IncrementViewCommand { get => new RelayCommand(IncrementView); }

        public ObservableCollection<string> ModOrganizerProfiles { get; set; } 
        public ObservableCollection<string> MissingArchives { get; set; }

        public string ModOrganizerExePath { get; set; }
        public string ModOrganizerProfilePath { get; set; }
        public string ModOrganizerCsv { get; set; }

        public SetupModpackViewModel(IComponentContext components)
        { 
            _transcompilerSetup = components.Resolve<ITranscompilerSetup>();
            _modListBuilder = components.Resolve<IModListBuilder>();
        }

        public void OpenDirectoryBrowser()
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Mod Organizer Executable|ModOrganizer.exe",
                Title = "Select your Mod Organizer Executable"
            };

            if (dialog.ShowDialog() == DialogResult.OK && File.Exists(dialog.FileName))
            {
                _transcompilerSetup.SetModOrganizerExePath(dialog.FileName);

                ModOrganizerExePath = dialog.FileName;
                ModOrganizerProfiles = new ObservableCollection<string>(_transcompilerSetup.GetModOrganizerProfiles());
            }
        }

        public void ContextMenuSelectionChanged(string contextMenuItem)
        {
            _transcompilerSetup.SetModOrganizerProfile(contextMenuItem);

            var (response, missingArchives) = _modListBuilder.BuildModListAndReturnMissing();

            if (missingArchives.Any())
            {
                MissingArchives = new ObservableCollection<string>(missingArchives);
            }
        }

        public void IncrementView()
        {

        }
    }
}
