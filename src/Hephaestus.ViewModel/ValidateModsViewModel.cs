using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class ValidateModsViewModel : ViewModelBase
    {
        private readonly IModListBuilder _modListBuilder;
        private readonly IViewIndexController _viewIndexController;

        public ObservableCollection<string> MissingArchives { get; set; }

        public ValidateModsViewModel(IComponentContext components)
        {
            _modListBuilder = components.Resolve<IModListBuilder>();
            _viewIndexController = components.Resolve<IViewIndexController>();

            _viewIndexController.ViewIndexChanged += (sender, i) => { BeginValidation(); };
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
