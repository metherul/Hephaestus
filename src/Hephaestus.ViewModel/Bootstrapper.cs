using System;
using Autofac;
using Autofac.Core;
using Hephaestus.Model.Interfaces;
using Hephaestus.ViewModel.Interfaces;

namespace Hephaestus.ViewModel
{
    public class Bootstrapper
    {
        private readonly ILifetimeScope _rootScope;

        // ViewModelLocator
        public IViewModel MainWindowViewModel => Resolve<IMainWindowViewModel>();
        public IViewModel NexusLoginViewModel => Resolve<INexusLoginViewModel>();
        public IViewModel MainPageViewModel => Resolve<IMainPageViewModel>();
        public IViewModel SetupModpackViewModel => Resolve<ISetupModpackViewModel>();
        public IViewModel ValidateModsViewModel => Resolve<IValidateModsViewModel>();
        public IViewModel TranscompilerViewModel => Resolve<ITranscompilerViewModel>();

        public Bootstrapper()
        {
            // Initialize Autofac instance
            var builder = new ContainerBuilder();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => typeof(IViewModel).IsAssignableFrom(t))
                .SingleInstance()
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => typeof(IModel).IsAssignableFrom(t))
                .Except<IService>()
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => typeof(IService).IsAssignableFrom(t))
                .AsImplementedInterfaces()
                .SingleInstance();

            _rootScope = builder.Build();
        }

        private T Resolve<T>()
        {
            return _rootScope.Resolve<T>(new Parameter[0]);
        }
    }
}
