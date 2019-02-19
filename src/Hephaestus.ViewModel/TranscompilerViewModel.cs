using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using Hephaestus.Model.Transcompiler.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class TranscompilerViewModel : ViewModelBase, ITranscompilerViewModel
    {
        private readonly IViewIndexController _viewIndexController;
        private readonly ITranscompile _transcompile;

        public ObservableCollection<string> ProgressLog { get; set; } = new ObservableCollection<string>();

        public TranscompilerViewModel(IComponentContext components)
        {
            _viewIndexController = components.Resolve<IViewIndexController>();
            _transcompile = components.Resolve<ITranscompile>();

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
            var progressLog = new Progress<string>();

            progressLog.ProgressChanged += (sender, s) =>
            {
                if (s.StartsWith("#"))
                {
                    ProgressLog = new ObservableCollection<string>();
                }

                ProgressLog.Add(s);
            };

            Task.Factory.StartNew(() =>
            {
                _transcompile.Start(progressLog);
            });
        }
    }

    public static class ScrollableScrollViewer
    {
        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(ScrollableScrollViewer), new PropertyMetadata(false, AutoScrollPropertyChanged));

        private static void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;

            if (scrollViewer != null && (bool)e.NewValue)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
