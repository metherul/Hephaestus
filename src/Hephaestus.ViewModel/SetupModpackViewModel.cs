using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private readonly IViewIndexController _viewIndexController;
        private readonly ITranscompilerSetup _transcompilerSetup;
        private readonly IModListBuilder _modListBuilder;

        public RelayCommand OpenDirectoryBrowserCommand => new RelayCommand(OpenDirectoryBrowser);
        public RelayCommand<string> ContextMenuSelectionChangedCommand => new RelayCommand<string>(ContextMenuSelectionChanged);
        public RelayCommand IncrementViewCommand => new RelayCommand(IncrementView);
        public RelayCommand<string> BrowseForArchiveCommand => new RelayCommand<string>(BrowseForArchive);
        public RelayCommand<string> SearchForArchiveCommand => new RelayCommand<string>(SearchForArchive);

        public ObservableCollection<string> ModOrganizerProfiles { get; set; } 
        public ObservableCollection<string> MissingArchives { get; set; }

        public string ModOrganizerExePath { get; set; }

        public string ModpackName { get; set; }
        public string ModpackAuthorName { get; set; }
        public string ModpackSource { get; set; }
        public string ModpackVersion { get; set; }

        public bool IsSetupComplete { get; set; }
        public bool HasInvalidMods { get; set; }

        public SetupModpackViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();
            _transcompilerSetup = components.Resolve<ITranscompilerSetup>();
            _modListBuilder = components.Resolve<IModListBuilder>();
        }

        public void OpenDirectoryBrowser()
        {
            ModOrganizerProfiles = new ObservableCollection<string>();

            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "Mod Organizer Executable|ModOrganizer.exe|All Files (*.*)|*.*",
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
            if (contextMenuItem == null) return;

            _transcompilerSetup.SetModOrganizerProfile(contextMenuItem);

            var missingArchives = _modListBuilder.BuildModListAndReturnMissing();

            IsSetupComplete = true;
        }

        public void BrowseForArchive(string archiveName)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = $"{archiveName}|{archiveName}|All Files (*.*)|*.*",
                Title = $"Search for: {archiveName}"
            };

            if (dialog.ShowDialog() != DialogResult.OK || !File.Exists(dialog.FileName)) return;

            var doMatch = _modListBuilder.ValidateArchiveAgainstPath(archiveName, dialog.FileName);

            if (!doMatch)
            {
                // Raise notification here
            }

            _modListBuilder.AddMissingArchive(archiveName, dialog.FileName);
            MissingArchives.Remove(archiveName);

            if (MissingArchives.Count == 0)
            {
                IsSetupComplete = true;
                HasInvalidMods = false;
            }
        }

        public void SearchForArchive(string archiveName)
        {
            Process.Start($"https://google.com/search?q={Path.GetFileNameWithoutExtension(archiveName)}");
        }

        public void IncrementView()
        {
            _transcompilerSetup.SetModpackName(ModpackName);
            _transcompilerSetup.SetModpackAuthorName(ModpackAuthorName);
            _transcompilerSetup.SetModpackSource(ModpackSource);
            _transcompilerSetup.SetModpackVersion(ModpackVersion);

            _viewIndexController.SetCurrentViewIndex(ViewIndex.ValidateMods);
        }
    }
}
