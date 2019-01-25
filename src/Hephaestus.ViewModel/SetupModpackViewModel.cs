using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Command;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class SetupModpackViewModel : ViewModelBase, ISetupModpackViewModel
    {
        private readonly ITranscompilerSetup _transcompilerSetup;

        public RelayCommand OpenDirectoryBrowserCommand { get => new RelayCommand(OpenDirectoryBrowser); }
        public RelayCommand ContinueImportCommand { get => new RelayCommand(ContinueImport); }

        public ObservableCollection<string> ModOrganizerProfiles { get; set; } 

        public string ModOrganizerExePath { get; set; }
        public string ModOrganizerProfilePath { get; set; }
        public string ModOrganizerCsv { get; set; }

        public SetupModpackViewModel(ITranscompilerSetup transcompilerSetup)
        {
            _transcompilerSetup = transcompilerSetup;
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

        public void ContinueImport()
        {

        }
    }
}
