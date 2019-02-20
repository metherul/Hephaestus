using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using GalaSoft.MvvmLight.Command;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class ValidateModsViewModel : ViewModelBase, IValidateModsViewModel
    {
        private readonly IModListBuilder _modListBuilder;
        private readonly IViewIndexController _viewIndexController;

        public RelayCommand<string> BrowseForArchiveCommand => new RelayCommand<string>(BrowseForArchive);
        public RelayCommand<string> SearchForArchiveCommand => new RelayCommand<string>(SearchForArchive);

        public ObservableCollection<string> MissingArchives { get; set; }

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

        public void BeginValidation()
        {
            Task.Factory.StartNew(() =>
            {
                MissingArchives = new ObservableCollection<string>(_modListBuilder.BuildModListAndReturnMissing());
            });
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
        }

        public void SearchForArchive(string archiveName)
        {
            Process.Start($"https://google.com/search?q={Path.GetFileNameWithoutExtension(archiveName)}");
        }
    }
}
