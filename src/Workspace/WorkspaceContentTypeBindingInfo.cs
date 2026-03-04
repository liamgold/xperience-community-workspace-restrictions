using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Workspaces;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(WorkspaceContentTypeBindingInfo), WorkspaceContentTypeBindingInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class WorkspaceContentTypeBindingInfo : AbstractInfo<WorkspaceContentTypeBindingInfo, IInfoProvider<WorkspaceContentTypeBindingInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.workspacecontenttypebinding";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<WorkspaceContentTypeBindingInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.WorkspaceContentTypeBinding",
        nameof(WorkspaceContentTypeBindingID),
        null, null, null, null, null,
        nameof(WorkspaceContentTypeBindingWorkspaceID),
        WorkspaceInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        IsBinding = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(WorkspaceContentTypeBindingClassID), DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
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
    public virtual int WorkspaceContentTypeBindingID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeBindingID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeBindingID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeBindingWorkspaceID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeBindingWorkspaceID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeBindingWorkspaceID), value);
    }

    [DatabaseField]
    public virtual int WorkspaceContentTypeBindingClassID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(WorkspaceContentTypeBindingClassID)), 0);
        set => SetValue(nameof(WorkspaceContentTypeBindingClassID), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public WorkspaceContentTypeBindingInfo() : base(TYPEINFO)
    {
    }

    public WorkspaceContentTypeBindingInfo(DataRow dr) : base(TYPEINFO, dr)
    {
    }
}
