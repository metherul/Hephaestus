using System;
using System.Reflection;
using Autofac;

namespace Hephaestus.ViewModel
{
    public class Bootstrapper
    {
        private readonly ILifetimeScope _rootLifetimeScope;

        public Bootstrapper()
        {
            var builder = new ContainerBuilder();
            var assembly = Assembly.GetCallingAssembly();

     
        }
    }
}
