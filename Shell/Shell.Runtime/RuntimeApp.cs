using Autofac;
using BassClefStudio.AppModel.Lifecycle;
using BassClefStudio.Shell.Runtime.Devices;
using BassClefStudio.Shell.Runtime.Processor;
using BassClefStudio.Shell.Runtime.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace BassClefStudio.Shell.Runtime
{
    public class RuntimeApp : App
    {
        public RuntimeApp() : base("BassClefStudio.Shell.Runtime", typeof(RuntimeApp).Assembly.GetName().Version)
        { }

        protected override void ConfigureServices(ContainerBuilder builder)
        {
            builder.RegisterViewModels(typeof(RuntimeApp).Assembly);
            builder.RegisterActivation(typeof(RuntimeApp).Assembly);
            builder.RegisterAssemblyTypes(typeof(RuntimeApp).Assembly)
                .AssignableTo<ISignal>()
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(RuntimeApp).Assembly)
                .AssignableTo<IDeviceDriver>()
                .PropertiesAutowired()
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(RuntimeApp).Assembly)
                .AssignableTo<ICore>()
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(RuntimeApp).Assembly)
                .AssignableTo<ICommandProvider>()
                .SingleInstance()
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(RuntimeApp).Assembly)
                .AssignableTo<ISystem>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .AsImplementedInterfaces();
            builder.RegisterType<RuntimeConfiguration>()
                .AsSelf()
                .SingleInstance();
        }
    }

    /// <summary>
    /// The default <see cref="IActivationHandler"/>.
    /// </summary>
    public class RuntimeActivationHandler : IActivationHandler
    {
        /// <inheritdoc/>
        public Type GetViewModel(IActivatedEventArgs args)
        {
            return typeof(MainViewModel);
        }
    }
}
