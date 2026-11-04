using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Workspaces;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(typeof(WorkspaceFolderScopeEditSection), "settings", typeof(WorkspaceFolderScopeSettingsPage),
    "Settings", TemplateNames.EDIT, UIPageOrder.First)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceFolderScopeSettingsPage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IInfoProvider<WorkspaceInfo> workspaceProvider,
    IInfoProvider<WorkspaceFolderScopeInfo> scopeProvider,
    IInfoProvider<ContentFolderInfo> folderProvider)
    : ModelEditPage<WorkspaceFolderScopeModel>(formItemCollectionProvider, formDataBinder)
{
    protected override WorkspaceFolderScopeModel Model { get; } = new();

    protected override bool RefetchAll => true;

    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceEditSection))]
    public int WorkspaceId { get; set; }

    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceFolderScopeEditSection))]
    public int EditedObjectId { get; set; }

    public override async Task<PageValidationResult> ValidatePage() => new()
    {
        IsValid = await GetScope() != null,
        ErrorMessageKey = "The folder scope does not belong to this workspace.",
    };

    public override async Task ConfigurePage()
    {
        PageConfiguration.Headline = "Settings";
        await base.ConfigurePage();
    }

    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var scope = await GetScope()
            ?? throw new InvalidOperationException("The folder scope does not belong to this workspace.");
        var workspace = await workspaceProvider.GetAsync(WorkspaceId)
            ?? throw new InvalidOperationException("The workspace no longer exists.");
        Model.FolderId = scope.WorkspaceFolderScopeFolderID;
        Model.ApplyToChildFolders = scope.WorkspaceFolderScopeApplyToChildFolders;
        var items = await base.GetFormItems();
        items.OfType<ContentFolderSelectorComponent>().Single().Properties.WorkspaceName = workspace.WorkspaceName;
        return items;
    }

    protected override async Task<ICommandResponse> ProcessFormData(WorkspaceFolderScopeModel model, ICollection<IFormItem> formItems)
    {
        var scope = await GetScope();
        if (scope == null)
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage("The folder scope does not belong to this workspace.");
        }

        var folder = await folderProvider.GetAsync(model.FolderId);
        if (folder == null || folder.ContentFolderWorkspaceID != WorkspaceId)
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage("Select a folder in this workspace.");
        }

        var existingScope = (await scopeProvider.Get()
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), WorkspaceId)
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID), model.FolderId)
            .WhereNotEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeID), EditedObjectId)
            .TopN(1)
            .GetEnumerableTypedResultAsync()).FirstOrDefault();

        if (existingScope != null)
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage(WorkspaceFolderScopePersistence.DuplicateFolderMessage);
        }

        scope.WorkspaceFolderScopeFolderID = model.FolderId;
        scope.WorkspaceFolderScopeApplyToChildFolders = model.ApplyToChildFolders;
        try
        {
            scopeProvider.Set(scope);
        }
        catch (Exception exception) when (WorkspaceFolderScopePersistence.IsDuplicateFolder(exception))
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage(WorkspaceFolderScopePersistence.DuplicateFolderMessage);
        }
        return await base.ProcessFormData(model, formItems);
    }

    protected override async Task<string> GetObjectDisplayName(WorkspaceFolderScopeModel model)
    {
        var folder = await folderProvider.GetAsync(model.FolderId);
        return folder?.ContentFolderDisplayName ?? "Folder scope";
    }

    private async Task<WorkspaceFolderScopeInfo?> GetScope()
    {
        var scope = await scopeProvider.GetAsync(EditedObjectId);
        if (scope == null || scope.WorkspaceFolderScopeWorkspaceID != WorkspaceId)
        {
            return null;
        }

        var folder = await folderProvider.GetAsync(scope.WorkspaceFolderScopeFolderID);
        return folder?.ContentFolderWorkspaceID == WorkspaceId ? scope : null;
    }
}
