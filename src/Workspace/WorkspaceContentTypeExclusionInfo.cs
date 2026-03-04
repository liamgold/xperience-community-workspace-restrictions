using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Workspaces;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(WorkspaceContentTypeExclusionInfo), WorkspaceContentTypeExclusionInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceContentTypeExclusionInfo : AbstractInfo<WorkspaceContentTypeExclusionInfo, IInfoProvider<WorkspaceContentTypeExclusionInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.workspacecontenttypeexclusion";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<WorkspaceContentTypeExclusionInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.WorkspaceContentTypeExclusion",
        nameof(WorkspaceContentTypeExclusionID),
        null, null, null, null, null,
        nameof(WorkspaceContentTypeExclusionWorkspaceID),
        WorkspaceInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        IsBinding = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(WorkspaceContentTypeExclusionClassID), DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
        },
        RegisterAsBindingToObjectTypes = new List<string>
        {
            WorkspaceInfo.OBJECT_TYPE,
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };

    [DatabaseField]
    public virtual int WorkspaceContentTypeExclusionID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeExclusionID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeExclusionID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeExclusionWorkspaceID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeExclusionWorkspaceID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeExclusionWorkspaceID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeExclusionClassID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeExclusionClassID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeExclusionClassID), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public WorkspaceContentTypeExclusionInfo() : base(TYPEINFO) { }

    public WorkspaceContentTypeExclusionInfo(DataRow dr) : base(TYPEINFO, dr) { }
}
