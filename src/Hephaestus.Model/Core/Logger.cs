using System;
using System.IO;
using System.Runtime.CompilerServices;
using Hephaestus.Model.Core.Interfaces;

namespace Hephaestus.Model.Core
{
    public class Logger : ILogger
    {
        public void Write(string message, [CallerMemberName] string callerName = "")
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"), $"[{callerName}] {message}");
        }
    }
}
