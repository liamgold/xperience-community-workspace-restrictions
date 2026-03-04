using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Workspaces;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(WorkspaceContentTypeExcludedInfo), WorkspaceContentTypeExcludedInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceContentTypeExcludedInfo : AbstractInfo<WorkspaceContentTypeExcludedInfo, IInfoProvider<WorkspaceContentTypeExcludedInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.workspacecontenttypeexcluded";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<WorkspaceContentTypeExcludedInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.WorkspaceContentTypeExcluded",
        nameof(WorkspaceContentTypeExcludedID),
        null, null, null, null, null,
        nameof(WorkspaceContentTypeExcludedWorkspaceID),
        WorkspaceInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        IsBinding = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(WorkspaceContentTypeExcludedClassID), DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
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
    public virtual int WorkspaceContentTypeExcludedID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeExcludedID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeExcludedID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeExcludedWorkspaceID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeExcludedWorkspaceID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeExcludedWorkspaceID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeExcludedClassID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeExcludedClassID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeExcludedClassID), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public WorkspaceContentTypeExcludedInfo() : base(TYPEINFO) { }

    public WorkspaceContentTypeExcludedInfo(DataRow dr) : base(TYPEINFO, dr) { }
}
