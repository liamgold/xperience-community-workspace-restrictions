using CMS;
using CMS.Core;
using CMS.Modules;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: CMS.RegisterModule(typeof(WorkspaceRestrictionsModule))]

namespace XperienceCommunity.WorkspaceRestrictions;

internal class WorkspaceRestrictionsModule : Module
{
    private WorkspaceContentTypeBindingInstaller? installer;

    public WorkspaceRestrictionsModule()
        : base(nameof(WorkspaceRestrictionsModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        installer = new WorkspaceContentTypeBindingInstaller(
            parameters.Services.GetRequiredService<IInfoProvider<ResourceInfo>>());

        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();
}
