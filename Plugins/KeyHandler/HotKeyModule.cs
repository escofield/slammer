using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using ES.LoggingTools;
using Plugins;

namespace KeyHandler
{
    public class HotKeyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HotKeyHandler>().As<IKeyHandler>();
        }

    }
}
