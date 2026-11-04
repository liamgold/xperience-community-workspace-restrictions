using CMS.Base;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: CMS.RegisterModule(typeof(WorkspaceRestrictionsModule))]

namespace XperienceCommunity.WorkspaceRestrictions;

internal class WorkspaceRestrictionsModule : Module
{
    private WorkspaceRestrictionsInstaller? installer;

    public WorkspaceRestrictionsModule()
        : base(nameof(WorkspaceRestrictionsModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        installer = new WorkspaceRestrictionsInstaller(
            parameters.Services.GetRequiredService<IInfoProvider<ResourceInfo>>());

        var services = parameters.Services;
        new ContentItemMoveGuard(
            new ContentTypeRestrictionResolver(
                services.GetRequiredService<IInfoProvider<WorkspaceContentTypeAllowedInfo>>(),
                services.GetRequiredService<IInfoProvider<WorkspaceContentTypeExcludedInfo>>(),
                services.GetRequiredService<IInfoProvider<WorkspaceFolderScopeInfo>>(),
                services.GetRequiredService<IInfoProvider<FolderScopeContentTypeAllowedInfo>>(),
                services.GetRequiredService<IInfoProvider<FolderScopeContentTypeExcludedInfo>>(),
                services.GetRequiredService<IInfoProvider<ContentFolderInfo>>()),
            services.GetRequiredService<IInfoProvider<ContentItemInfo>>(),
            services.GetService<IHttpContextAccessor>())
            .Register();

        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();
}
