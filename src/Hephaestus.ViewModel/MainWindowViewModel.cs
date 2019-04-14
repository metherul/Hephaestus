using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using GalaSoft.MvvmLight.Command;
using Hephaestus.Model.Core.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private IViewIndexController _viewIndexController;
        private readonly ILogger _logger;

        public RelayCommand<Window> CloseWindowCommand { get; set; }

        public int CurrentViewIndex { get; set; }

        public MainWindowViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();
            _logger = components.Resolve<ILogger>();

            _viewIndexController.ViewIndexChanged += _viewIndexController_ViewIndexChanged;

            CloseWindowCommand = new RelayCommand<Window>(CloseWindow);

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt")))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"));
            }

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                _logger.Write(eventArgs.Exception.Message + "\n");
            };

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                _logger.Write(eventArgs.Exception.Message + "\n");
            };


        }

        private void _viewIndexController_ViewIndexChanged(object sender, int e)
        {
            CurrentViewIndex = e;
        }


        private static void CloseWindow(Window window)
        {
            window.Close();
        }
    }
}
