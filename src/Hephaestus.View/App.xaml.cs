using System;
using System.IO;
using System.Windows;

namespace Hephaestus.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt")))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"));
            }

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                File.AppendText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"));
            };
        }
    }
}
