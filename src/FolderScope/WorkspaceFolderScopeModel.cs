using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceFolderScopeModel
{
    [ContentFolderSelectorComponent(Label = "Folder", Order = 0,
        ExplanationText = "Content types must also be allowed by the workspace and any inherited folder scopes.")]
    [RequiredValidationRule]
    public int FolderId { get; set; }

    [CheckBoxComponent(Label = "Apply to child folders", Order = 1,
        ExplanationText = "Apply these restrictions to all nested folders. Child scopes can further restrict the content types allowed by this scope.")]
    public bool ApplyToChildFolders { get; set; }
}
