using System;
using System.IO;
using Autofac;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private IViewIndexController _viewIndexController;
        private readonly ILogger _logger;

        public int CurrentViewIndex { get; set; }

        public MainWindowViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();
            _logger = components.Resolve<ILogger>();

            _viewIndexController.ViewIndexChanged += _viewIndexController_ViewIndexChanged;

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt")))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"));
            }

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                _logger.Write(eventArgs.Exception.Message + "\n");
            };
        }

        private void _viewIndexController_ViewIndexChanged(object sender, int e)
        {
            CurrentViewIndex = e;
        }
    }
}
