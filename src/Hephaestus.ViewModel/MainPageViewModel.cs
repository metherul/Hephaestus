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
        }

        private void IncrementView(ViewIndex viewIndex)
        {
            _viewIndexController.SetCurrentViewIndex(viewIndex);
        }
    }
}
