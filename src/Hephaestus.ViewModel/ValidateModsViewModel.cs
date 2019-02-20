using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class ValidateModsViewModel : ViewModelBase, IValidateModsViewModel
    {
        private readonly IModListBuilder _modListBuilder;
        private readonly IViewIndexController _viewIndexController;

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
    }
}
