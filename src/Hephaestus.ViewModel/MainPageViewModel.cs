using System;
using System.IO;
using Autofac;
using GalaSoft.MvvmLight.Command;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class MainPageViewModel : ViewModelBase, IMainPageViewModel
    {
        private readonly IViewIndexController _viewIndexController;

        public RelayCommand<ViewIndex> IncrementViewCommand { get => new RelayCommand<ViewIndex>(IncrementView); }

        public MainPageViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt")))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"));
            }

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"), eventArgs.Exception.Message);
            };
        }

        private void IncrementView(ViewIndex viewIndex)
        {
            _viewIndexController.SetCurrentViewIndex(viewIndex);
        }
    }
}
