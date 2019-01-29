using System;

namespace Hephaestus.ViewModel.Interfaces
{
    public interface IViewIndexController : IViewModel
    {
        event EventHandler<int> ViewIndexChanged;

        void SetCurrentViewIndex(ViewIndex viewIndex);
    }
}