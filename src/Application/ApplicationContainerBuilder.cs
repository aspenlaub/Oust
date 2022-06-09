using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Oust.Application.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Oust.Model.IO;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Entities.Web;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Helpers;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNet.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb;
using Aspenlaub.Net.GitHub.CSharp.VishizhukelNetWeb.Interfaces;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Oust.Application;

public static class ApplicationContainerBuilder {
    public static async Task<ContainerBuilder> RegisterForOustApplicationAsync(this ContainerBuilder builder) {
        await builder.UseVishizhukelNetWebDvinAndPeghAsync("Oust", new DummyCsArgumentPrompter());
        builder.RegisterType<ButtonNameToCommandMapper>().As<IButtonNameToCommandMapper>().SingleInstance();
        builder.RegisterInstance<IContextFactory>(new ContextFactory());
        builder.RegisterInstance<IDumperNameConverter>(new DumperNameConverter());
        builder.RegisterType<NewScriptNameValidator>().As<INewScriptNameValidator>();
        builder.RegisterType<ObsoleteScriptChecker>().As<IObsoleteScriptChecker>();
        builder.RegisterType<Application>().As<Application>().As<IApplication>().As<IGuiAndWebViewAppHandler<ApplicationModel>>().SingleInstance();
        builder.RegisterType<ScriptAndSubScriptsConsolidator>().As<IScriptAndSubScriptsConsolidator>();
        builder.RegisterType<SubScriptExtractor>().As<ISubScriptExtractor>();
        builder.RegisterType<ExecutionStackFormatter>().As<IExecutionStackFormatter>();
        builder.RegisterType<FileDialogTrickster>().As<IFileDialogTrickster>();
        builder.RegisterType<OustScriptStatementFactory>().As<IOustScriptStatementFactory>();

        var container = new ContainerBuilder().UsePegh("Oust", new DummyCsArgumentPrompter()).Build();
        var folderResolver = container.Resolve<IFolderResolver>();
        var secretRepository = container.Resolve<ISecretRepository>();
        var errorsAndInfos = new ErrorsAndInfos();
        var logFolder = await folderResolver.ResolveAsync(@"$(WampLog)", errorsAndInfos);
        var extendedLogFolder = await folderResolver.ResolveAsync(@"$(WampExtLog)", errorsAndInfos);
        var wampLogFolder = await folderResolver.ResolveAsync(@"$(WampCoreLog)", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        builder.Register(_ => new WampLogScanner(logFolder, extendedLogFolder, wampLogFolder)).As<IWampLogScanner>();

        var securedHttpGateSettingsSecret = new SecretSecuredHttpGateSettings();
        var securedHttpGateSettings = await secretRepository.GetAsync(securedHttpGateSettingsSecret, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }

        builder.RegisterInstance(securedHttpGateSettings);

        return builder;
    }
}