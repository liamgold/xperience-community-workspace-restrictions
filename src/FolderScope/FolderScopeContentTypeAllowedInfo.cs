using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(FolderScopeContentTypeAllowedInfo), FolderScopeContentTypeAllowedInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class FolderScopeContentTypeAllowedInfo : AbstractInfo<FolderScopeContentTypeAllowedInfo, IInfoProvider<FolderScopeContentTypeAllowedInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.folderscopecontenttypeallowed";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<FolderScopeContentTypeAllowedInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.FolderScopeContentTypeAllowed",
        nameof(FolderScopeContentTypeAllowedID),
        null, null, null, null, null,
        nameof(FolderScopeContentTypeAllowedFolderScopeID),
        WorkspaceFolderScopeInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        IsBinding = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(FolderScopeContentTypeAllowedClassID), DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
        },
        RegisterAsBindingToObjectTypes = new List<string>
        {
            WorkspaceFolderScopeInfo.OBJECT_TYPE,
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };

    [DatabaseField]
    public virtual int FolderScopeContentTypeAllowedID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FolderScopeContentTypeAllowedID)), 0);
        set => SetValue(nameof(FolderScopeContentTypeAllowedID), value);
    }

    [DatabaseField]
    public virtual int FolderScopeContentTypeAllowedFolderScopeID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FolderScopeContentTypeAllowedFolderScopeID)), 0);
        set => SetValue(nameof(FolderScopeContentTypeAllowedFolderScopeID), value);
    }

    [DatabaseField]
    public virtual int FolderScopeContentTypeAllowedClassID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FolderScopeContentTypeAllowedClassID)), 0);
        set => SetValue(nameof(FolderScopeContentTypeAllowedClassID), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public FolderScopeContentTypeAllowedInfo() : base(TYPEINFO)
    {
    }

    public FolderScopeContentTypeAllowedInfo(DataRow dr) : base(TYPEINFO, dr)
    {
    }
}
