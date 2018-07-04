using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autofac;
using Autofac.Configuration;

using ES.LoggingTools;



namespace TestPlan
{
    [TestClass]
    public class UnitTest1
    {
        private IContainer _container;
        private ILifetimeScope _autoFac;

        [TestInitialize()]
        public void Initialize()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule());
            //builder.RegisterModule(new ConfigurationSettingsReader("autofac"));
           
            _container = builder.Build();
            _autoFac = _container.BeginLifetimeScope();

        }

        [TestMethod]
        public void TestLogging()
        {
            var x = _container.Resolve<ILog>(new TypedParameter(typeof(Type),this.GetType()));
            x.Debug("test");
        }
    }
}
