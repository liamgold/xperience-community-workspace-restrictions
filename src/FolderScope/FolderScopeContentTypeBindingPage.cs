using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

namespace XperienceCommunity.WorkspaceRestrictions;

public abstract class FolderScopeContentTypeBindingPage<TBindingInfo>(
    IInfoProvider<WorkspaceContentTypeAllowedInfo> allowedProvider,
    IInfoProvider<WorkspaceContentTypeExcludedInfo> excludedProvider)
    : InfoBindingPage<TBindingInfo, DataClassInfo>
    where TBindingInfo : BaseInfo, new()
{
    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceFolderScopeEditSection))]
    public override int EditedObjectId { get; set; }

    [PageParameter(typeof(IntPageModelBinder), typeof(WorkspaceEditSection))]
    public int WorkspaceId { get; set; }

    protected abstract string Caption { get; }

    public override async Task<PageValidationResult> ValidatePage()
    {
        var scope = await WorkspaceFolderScopeInfo.Provider.GetAsync(EditedObjectId);
        var folder = scope == null ? null : await ContentFolderInfo.Provider.GetAsync(scope.WorkspaceFolderScopeFolderID);
        return new PageValidationResult
        {
            IsValid = scope?.WorkspaceFolderScopeWorkspaceID == WorkspaceId && folder?.ContentFolderWorkspaceID == WorkspaceId,
            ErrorMessageKey = "The folder scope does not belong to this workspace.",
        };
    }

    public override async Task ConfigurePage()
    {
        var restriction = await ContentTypeRestriction.ForWorkspace(WorkspaceId, allowedProvider, excludedProvider);

        PageConfiguration.ExistingBindingsListing.Caption = Caption;
        PageConfiguration.BindingSidePanelListing.Caption = "Content types allowed by the workspace";
        PageConfiguration.AddBindingButtonText = "Add content type";
        PageConfiguration.SaveBindingsButtonText = "Save";

        PageConfiguration.ExistingBindingsListing.ColumnConfigurations
            .AddColumn(nameof(DataClassInfo.ClassDisplayName), caption: "Display name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);
        PageConfiguration.BindingSidePanelListing.ColumnConfigurations
            .AddColumn(nameof(DataClassInfo.ClassDisplayName), caption: "Display name", searchable: true, defaultSortDirection: SortTypeEnum.Asc);

        // Keep existing bindings visible so editors can remove selections disallowed by a later workspace change.
        PageConfiguration.BindingSidePanelListing.QueryModifiers.AddModifier(query => restriction.ApplyTo(query)
            .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.REUSABLE));

        await base.ConfigurePage();
    }

    protected override bool ValidateBindings(ICollection<int> identifiers)
    {
        var scope = WorkspaceFolderScopeInfo.Provider.Get(EditedObjectId);
        var folder = scope == null ? null : ContentFolderInfo.Provider.Get(scope.WorkspaceFolderScopeFolderID);
        if (scope?.WorkspaceFolderScopeWorkspaceID != WorkspaceId || folder?.ContentFolderWorkspaceID != WorkspaceId)
        {
            return false;
        }

        // The base implementation also applies the side-panel query modifiers when validating submitted IDs.
        return base.ValidateBindings(identifiers);
    }

    [PageCommand(Permission = SystemPermissions.UPDATE)]
    public override async Task<ICommandResponse<RowActionResult>> RemoveBinding(int bindingId)
    {
        var binding = ProviderHelper.GetInfoById(new TBindingInfo().TypeInfo.ObjectType, bindingId);
        if (!(await ValidatePage()).IsValid || binding == null || binding.GetIntegerValue(SourceBindingColumn, 0) != EditedObjectId)
        {
            return ResponseFrom(new RowActionResult(false)).AddErrorMessage("The content type binding does not belong to this folder scope.");
        }

        return await base.RemoveBinding(bindingId);
    }
}
