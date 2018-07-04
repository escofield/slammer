using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ES.LoggingTools;
using ES.Windows;
using Plugins;
using KeyHandler;
using Autofac;
using Autofac.Configuration;

namespace Slammer
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern Boolean AllocConsole();
    }

    static class Slam
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Slammer>().As<Slammer>();
            builder.RegisterType<WindowTools>().As<WindowTools>();
            builder.RegisterType<ProcessMem>().As<ProcessMem>();
            builder.RegisterType<KeyboardHook>().As<KeyboardHook>();
            builder.RegisterModule(new ConfigurationSettingsReader("autofac"));
            builder.RegisterType<SlammerUIHandler>().As<SlammerUIHandler>();

            var container = builder.Build();
            var autoFac = container.BeginLifetimeScope();

            NativeMethods.AllocConsole();           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(container.Resolve<Slammer>());

            autoFac.Dispose();
            container.Dispose();
        }
    }
}
