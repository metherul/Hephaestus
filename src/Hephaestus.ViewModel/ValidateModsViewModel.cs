using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using GalaSoft.MvvmLight.Command;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;
using Ookii.Dialogs.Wpf;

namespace Hephaestus.ViewModel
{
    public class ValidateModsViewModel : ViewModelBase, IValidateModsViewModel
    {
        private readonly IModListBuilder _modListBuilder;
        private readonly IViewIndexController _viewIndexController;

        public RelayCommand<string> BrowseForArchiveCommand => new RelayCommand<string>(BrowseForArchive);
        public RelayCommand<string> SearchForArchiveCommand => new RelayCommand<string>(SearchForArchive);
        public RelayCommand ValidateDirectoryCommand => new RelayCommand(ValidateDirectory);
        public RelayCommand IncrementViewCommand => new RelayCommand(IncrementView);

        public ObservableCollection<string> MissingArchives { get; set; }

        public bool IsValidationComplete { get; set; }
        public bool IsValidating { get; set; }

        public ValidateModsViewModel(IComponentContext components)
        {
            _modListBuilder = components.Resolve<IModListBuilder>();
            _viewIndexController = components.Resolve<IViewIndexController>();

            _viewIndexController.ViewIndexChanged += (sender, i) =>
            {
                if (i == Convert.ToInt32(ViewIndex.ValidateMods))
                {
                    BeginValidation();
                }
            };
        }

        private async void BeginValidation()
        {
            await Task.Factory.StartNew(() =>
            {
                MissingArchives = new ObservableCollection<string>(_modListBuilder.BuildModListAndReturnMissing());
            });

            IsValidationComplete = MissingArchives.Count == 0;
        }

        private void BrowseForArchive(string archiveName)
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
                IsValidationComplete = true;
            }
        }

        private void SearchForArchive(string archiveName)
        {
            Process.Start($"https://google.com/search?q={Path.GetFileNameWithoutExtension(archiveName)}");
        }

        private async void ValidateDirectory()
        {
            var directoryBrowser = new VistaFolderBrowserDialog()
            {
                Description = "Select a folder to analyze.",
                UseDescriptionForTitle = true
            };

            if (directoryBrowser.ShowDialog() == true)
            {
                await Task.Factory.StartNew(() =>
                {
                    MissingArchives = new ObservableCollection<string>(_modListBuilder.AnalyzeDirectory(MissingArchives.ToList(), directoryBrowser.SelectedPath));
                });

                IsValidationComplete = MissingArchives.Count == 0;
            }
        }

        private void IncrementView()
        {
            _viewIndexController.SetCurrentViewIndex(ViewIndex.Transcompiler);
        }
    }
}
