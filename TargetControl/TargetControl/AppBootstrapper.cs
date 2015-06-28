using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;

using SimpleInjector;

using TargetControl.Models;

namespace TargetControl
{
    public class AppBootstrapper : BootstrapperBase
    {
        private Container _container = new Container();

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container.Register<IWindowManager, WindowManager>();
            _container.RegisterSingle<IEventAggregator, EventAggregator>();

            // stubs
            var useStubs = Environment.GetCommandLineArgs().Contains("/stubs");
            if (!useStubs)
            {
                _container.RegisterSingle<ISerialPortListProvider, SerialPortListProvider>();
                _container.RegisterSingle<ISerial, SerialPortSerial>();
            }
            else
            {
                _container.RegisterSingle<ISerialPortListProvider, StubSerialPortListProvider>();
                _container.RegisterSingle<ISerial, StubSerialPort>();
            }

            _container.Register<DispatcherTimer>(() => new DispatcherTimer());
            _container.RegisterSingle<ISerialPacketParser, SerialPacketParser>();
            _container.RegisterSingle<ISerialCommandInterface, SerialCommandInterface>();
            _container.RegisterSingle<ITargetHitManager, TargetHitManager>();
            _container.RegisterSingle<ITeamDatabaseSerializer, TeamDatabaseSerializer>();

            _container.RegisterAll(typeof(IMainScreenTabItem), new[]
                {
                    typeof(TeamsViewModel),
                    typeof(ContestViewModel),
                    typeof(ManualControlViewModel),
                    typeof(TerminalViewModel),
                });

            _container.Options.AllowResolvingFuncFactories();

            _container.Verify();
        }
        
        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            var registration = _container.GetRegistration(instance.GetType(), true);
            registration.Registration.InitializeInstance(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }

    public static class SimpleInjectorExtensions
    {
        public static void AllowResolvingFuncFactories(this ContainerOptions options)
        {
            options.Container.ResolveUnregisteredType += (s, e) => {
                var type = e.UnregisteredServiceType;

                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Func<>))
                {
                    return;
                }

                Type serviceType = type.GetGenericArguments().First();

                InstanceProducer registration =
                    options.Container.GetRegistration(serviceType, true);

                Type funcType = typeof(Func<>).MakeGenericType(serviceType);

                var factoryDelegate = System.Linq.Expressions.Expression.Lambda(funcType,
                    registration.BuildExpression()).Compile();

                e.Register(System.Linq.Expressions.Expression.Constant(factoryDelegate));
            };
        }
    }
}
