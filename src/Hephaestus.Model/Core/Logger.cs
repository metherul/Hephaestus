using System;
using System.IO;
using Hephaestus.Model.Core.Interfaces;

namespace Hephaestus.Model.Core
{
    public class Logger : ILogger
    {
        public void Write(string message)
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"), message);
        }
    }
}
