using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Workspaces;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.WorkspaceRestrictions;

[assembly: UIPage(typeof(WorkspaceFolderScopeListingPage), "create", typeof(WorkspaceFolderScopeCreatePage),
    "New folder scope", TemplateNames.EDIT, UIPageOrder.NoOrder)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceFolderScopeCreatePage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IPageLinkGenerator pageLinkGenerator,
    IInfoProvider<WorkspaceInfo> workspaceProvider,
    IInfoProvider<ContentFolderInfo> folderProvider,
    IInfoProvider<WorkspaceFolderScopeInfo> scopeProvider)
    : ModelEditPage<WorkspaceFolderScopeModel>(formItemCollectionProvider, formDataBinder)
{
    protected override WorkspaceFolderScopeModel Model { get; } = new();

    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceEditSection))]
    public int WorkspaceId { get; set; }

    public override async Task ConfigurePage()
    {
        PageConfiguration.Headline = "New folder scope";
        await base.ConfigurePage();
    }

    protected override async Task<ICollection<IFormItem>> GetFormItems()
    {
        var workspace = await workspaceProvider.GetAsync(WorkspaceId)
            ?? throw new InvalidOperationException("The workspace no longer exists.");
        var items = await base.GetFormItems();
        items.OfType<ContentFolderSelectorComponent>().Single().Properties.WorkspaceName = workspace.WorkspaceName;

        return items;
    }

    protected override async Task<ICommandResponse> ProcessFormData(WorkspaceFolderScopeModel model, ICollection<IFormItem> formItems)
    {
        var folder = await folderProvider.GetAsync(model.FolderId);
        if (folder == null || folder.ContentFolderWorkspaceID != WorkspaceId)
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage("Select a folder in this workspace.");
        }

        var existingScope = (await scopeProvider.Get()
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), WorkspaceId)
            .WhereEquals(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID), model.FolderId)
            .TopN(1)
            .GetEnumerableTypedResultAsync()).FirstOrDefault();

        if (existingScope != null)
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage(WorkspaceFolderScopePersistence.DuplicateFolderMessage);
        }

        var scope = new WorkspaceFolderScopeInfo
        {
            WorkspaceFolderScopeGUID = Guid.NewGuid(),
            WorkspaceFolderScopeWorkspaceID = WorkspaceId,
            WorkspaceFolderScopeFolderID = model.FolderId,
            WorkspaceFolderScopeApplyToChildFolders = model.ApplyToChildFolders,
        };
        try
        {
            scopeProvider.Set(scope);
        }
        catch (Exception exception) when (WorkspaceFolderScopePersistence.IsDuplicateFolder(exception))
        {
            return ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                .AddErrorMessage(WorkspaceFolderScopePersistence.DuplicateFolderMessage);
        }

        return NavigateTo(pageLinkGenerator.GetPath<FolderScopeContentTypeAllowedPage>(new PageParameterValues
        {
            { typeof(WorkspaceEditSection), WorkspaceId },
            { typeof(WorkspaceFolderScopeEditSection), scope.WorkspaceFolderScopeID },
        }));
    }
}
