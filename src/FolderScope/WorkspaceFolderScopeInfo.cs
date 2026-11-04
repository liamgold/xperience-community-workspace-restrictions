using System.Data;

using CMS;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Workspaces;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(WorkspaceFolderScopeInfo), WorkspaceFolderScopeInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceFolderScopeInfo : AbstractInfo<WorkspaceFolderScopeInfo, IInfoProvider<WorkspaceFolderScopeInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.workspacefolderscope";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<WorkspaceFolderScopeInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.WorkspaceFolderScope",
        nameof(WorkspaceFolderScopeID),
        timeStampColumn: null,
        guidColumn: nameof(WorkspaceFolderScopeGUID),
        codeNameColumn: null,
        displayNameColumn: null,
        binaryColumn: null,
        parentIDColumn: nameof(WorkspaceFolderScopeWorkspaceID),
        parentObjectType: WorkspaceInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(WorkspaceFolderScopeFolderID), ContentFolderInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };

    [DatabaseField]
    public virtual int WorkspaceFolderScopeID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceFolderScopeID)), 0);
        set => SetValue(nameof(WorkspaceFolderScopeID), value);
    }

    [DatabaseField]
    public virtual Guid WorkspaceFolderScopeGUID
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(WorkspaceFolderScopeGUID)), Guid.Empty);
        set => SetValue(nameof(WorkspaceFolderScopeGUID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceFolderScopeWorkspaceID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceFolderScopeWorkspaceID)), 0);
        set => SetValue(nameof(WorkspaceFolderScopeWorkspaceID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceFolderScopeFolderID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceFolderScopeFolderID)), 0);
        set => SetValue(nameof(WorkspaceFolderScopeFolderID), value);
    }

    [DatabaseField]
    public virtual bool WorkspaceFolderScopeApplyToChildFolders
    {
        get => ValidationHelper.GetBoolean(GetValue(nameof(WorkspaceFolderScopeApplyToChildFolders)), false);
        set => SetValue(nameof(WorkspaceFolderScopeApplyToChildFolders), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public WorkspaceFolderScopeInfo() : base(TYPEINFO) { }

    public WorkspaceFolderScopeInfo(DataRow dr) : base(TYPEINFO, dr) { }
}
