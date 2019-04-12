using Autofac;
using Hephaestus.Model.Transcompiler;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hephaestus.ViewModel
{
    public class PostTranscompilationViewModel : ViewModelBase, IPostTranscompilationViewModel
    {
        private readonly IViewIndexController _viewController;
        private readonly ITranscompilerBase _transcompilerBase;

        public ObservableCollection<IntermediaryModObject> ModsWithIssue { get; set; }

        public PostTranscompilationViewModel(IComponentContext components)
        {
            _viewController = components.Resolve<IViewIndexController>();
            _transcompilerBase = components.Resolve<ITranscompilerBase>();

            _viewController.ViewIndexChanged += _viewController_ViewIndexChanged;
        }

        private void _viewController_ViewIndexChanged(object sender, int e)
        {
            if (e == (int)ViewIndex.PostTranscompilation)
            {
                ModsWithIssue = new ObservableCollection<IntermediaryModObject>(_transcompilerBase.IntermediaryModObjects.Where(x => x.Inconsistencies.Any()));
            }
        }
    }
}
