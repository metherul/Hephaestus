using Autofac;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private IViewIndexController _viewIndexController;

        public int CurrentViewIndex { get; set; }

        public MainWindowViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();

            _viewIndexController.ViewIndexChanged += _viewIndexController_ViewIndexChanged;
        }

        private void _viewIndexController_ViewIndexChanged(object sender, int e)
        {
            CurrentViewIndex = e;
        }
    }
}
