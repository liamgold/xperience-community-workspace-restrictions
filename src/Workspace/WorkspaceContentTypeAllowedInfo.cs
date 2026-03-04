using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Workspaces;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(WorkspaceContentTypeAllowedInfo), WorkspaceContentTypeAllowedInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceContentTypeAllowedInfo : AbstractInfo<WorkspaceContentTypeAllowedInfo, IInfoProvider<WorkspaceContentTypeAllowedInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.workspacecontenttypeallowed";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<WorkspaceContentTypeAllowedInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.WorkspaceContentTypeAllowed",
        nameof(WorkspaceContentTypeAllowedID),
        null, null, null, null, null,
        nameof(WorkspaceContentTypeAllowedWorkspaceID),
        WorkspaceInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        IsBinding = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(WorkspaceContentTypeAllowedClassID), DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
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
    public virtual int WorkspaceContentTypeAllowedID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeAllowedID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeAllowedID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeAllowedWorkspaceID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeAllowedWorkspaceID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeAllowedWorkspaceID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeAllowedClassID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeAllowedClassID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeAllowedClassID), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public WorkspaceContentTypeAllowedInfo() : base(TYPEINFO)
    {
    }

    public WorkspaceContentTypeAllowedInfo(DataRow dr) : base(TYPEINFO, dr)
    {
    }
}
