using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Components;
using Aspenlaub.Net.GitHub.CSharp.TashClient.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Components;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishnetIntegrationTestTools;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Integration.Test;

public static class OustIntegrationTestContainerBuilder {
    public static ContainerBuilder RegisterForOustIntegrationTest(this ContainerBuilder builder) {
        builder.UseDvinAndPegh("Oust");
        builder.RegisterType<ContextFactory>().As<IContextFactory>();
        builder.RegisterType<OustStarterAndStopper>().As<IStarterAndStopper>();
        builder.RegisterType<OustWindowUnderTest>();
        builder.RegisterInstance<IDumperNameConverter>(new DumperNameConverter());
        builder.RegisterType<TashAccessor>().As<ITashAccessor>();
        builder.RegisterType<LogicalUrlRepository>().As<ILogicalUrlRepository>();
        return builder;
    }
}