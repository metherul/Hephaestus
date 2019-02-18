using System;
using Autofac;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class TranscompilerViewModel : ITranscompilerViewModel
    {
        private readonly IViewIndexController _viewIndexController;

        public TranscompilerViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();

            _viewIndexController.ViewIndexChanged += (sender, i) =>
            {
                if (i == Convert.ToInt32(ViewIndex.Transcompiler))
                {
                    BeginTranscompile();
                }
            };
        }

        public void BeginTranscompile()
        {

        }
    }
}
