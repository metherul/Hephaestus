using System;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class ViewIndexController : IViewIndexController
    {
        public event EventHandler<int> ViewIndexChanged;

        public void SetCurrentViewIndex(ViewIndex viewIndex)
        {
            ViewIndexChanged.Invoke(this, (int)viewIndex);
        }
    }
}
