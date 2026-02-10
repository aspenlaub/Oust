using System;
using System.Threading.Tasks;
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
    public static async Task<ContainerBuilder> RegisterForOustApplicationAsync(this ContainerBuilder builder, EnvironmentType environmentType) {
        await builder.UseVishizhukelNetWebDvinAndPeghAsync("Oust");
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

        IContainer container = new ContainerBuilder().UsePegh("Oust").Build();
        IFolderResolver folderResolver = container.Resolve<IFolderResolver>();
        ISecretRepository secretRepository = container.Resolve<ISecretRepository>();
        var errorsAndInfos = new ErrorsAndInfos();
        IFolder logFolder = await folderResolver.ResolveAsync(@"$(WampLog)", errorsAndInfos);
        IFolder extendedLogFolder = await folderResolver.ResolveAsync(@"$(WampExtLog)", errorsAndInfos);
        IFolder wampLogFolder = await folderResolver.ResolveAsync(@"$(WampCoreLog)", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        builder.Register(_ => new WampLogScanner(logFolder, extendedLogFolder, wampLogFolder, environmentType)
            ).As<IWampLogScanner>();

        var securedHttpGateSettingsSecret = new SecretSecuredHttpGateSettings();
        SecuredHttpGateSettings securedHttpGateSettings = await secretRepository.GetAsync(securedHttpGateSettingsSecret, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }

        builder.RegisterInstance(securedHttpGateSettings);

        return builder;
    }
}