using System;
using System.IO;
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

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt")))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"));
            }

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"), eventArgs.Exception.Message);
            };
        }

        private void _viewIndexController_ViewIndexChanged(object sender, int e)
        {
            CurrentViewIndex = e;
        }
    }
}
