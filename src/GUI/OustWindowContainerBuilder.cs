using Aspenlaub.Net.GitHub.CSharp.Oust.Application;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Handlers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.GUI;

public static class OustWindowContainerBuilder {
    public static ContainerBuilder RegisterForOustWindow(this ContainerBuilder builder, EnvironmentType environmentType, OustWindow oustWindow) {
        builder.RegisterType<GuiToApplicationGate>().As<IGuiToWebViewApplicationGate>();
        builder.RegisterType<TashCommunicator>().As<ITashCommunicator<IApplicationModel>>();
        builder.RegisterType<ShowExecutionStackPopupFactory>().As<IShowExecutionStackPopupFactory>();

        builder.RegisterType<Dumper>().WithParameter((p, _) => p.ParameterType == typeof(EnvironmentType), (_, _) => environmentType) .As<IDumper>();
        builder.RegisterType<Importer>().WithParameter((p, _) => p.ParameterType == typeof(EnvironmentType), (_, _) => environmentType).As<IImporter>();
        builder.RegisterType<DumpFolderChecker>().WithParameter((p, _) => p.ParameterType == typeof(EnvironmentType), (_, _) => environmentType).As<IDumpFolderChecker>();

        builder.RegisterType<GuiAndApplicationSynchronizer>()
            .WithParameter((p, _) => p.ParameterType == typeof(OustWindow), (_, _) => oustWindow)
            .As<IGuiAndApplicationSynchronizer>();

        builder.RegisterType<ApplicationModel>()
            .WithParameter((p, _) => p.ParameterType == typeof(EnvironmentType), (_, _) => environmentType)
            .As<IApplicationModel>()
            .As<IApplicationModelBase>()
            .As<IBusy>()
            .As<ApplicationModel>()
            .SingleInstance();

        return builder;
    }
}