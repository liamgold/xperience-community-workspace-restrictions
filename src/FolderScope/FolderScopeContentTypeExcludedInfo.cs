using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using XperienceCommunity.WorkspaceRestrictions;

[assembly: RegisterObjectType(typeof(FolderScopeContentTypeExcludedInfo), FolderScopeContentTypeExcludedInfo.OBJECT_TYPE)]

namespace XperienceCommunity.WorkspaceRestrictions;

public class FolderScopeContentTypeExcludedInfo : AbstractInfo<FolderScopeContentTypeExcludedInfo, IInfoProvider<FolderScopeContentTypeExcludedInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "xperiencecommunity.folderscopecontenttypeexcluded";

    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
        typeof(IInfoProvider<FolderScopeContentTypeExcludedInfo>),
        OBJECT_TYPE,
        "XperienceCommunity.FolderScopeContentTypeExcluded",
        nameof(FolderScopeContentTypeExcludedID),
        null, null, null, null, null,
        nameof(FolderScopeContentTypeExcludedFolderScopeID),
        WorkspaceFolderScopeInfo.OBJECT_TYPE)
    {
        TouchCacheDependencies = true,
        IsBinding = true,
        DependsOn = new List<ObjectDependency>
        {
            new ObjectDependency(nameof(FolderScopeContentTypeExcludedClassID), DataClassInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
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
    public virtual int FolderScopeContentTypeExcludedID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FolderScopeContentTypeExcludedID)), 0);
        set => SetValue(nameof(FolderScopeContentTypeExcludedID), value);
    }

    [DatabaseField]
    public virtual int FolderScopeContentTypeExcludedFolderScopeID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FolderScopeContentTypeExcludedFolderScopeID)), 0);
        set => SetValue(nameof(FolderScopeContentTypeExcludedFolderScopeID), value);
    }

    [DatabaseField]
    public virtual int FolderScopeContentTypeExcludedClassID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FolderScopeContentTypeExcludedClassID)), 0);
        set => SetValue(nameof(FolderScopeContentTypeExcludedClassID), value);
    }

    protected override void DeleteObject() => Provider.Delete(this);

    protected override void SetObject() => Provider.Set(this);

    public FolderScopeContentTypeExcludedInfo() : base(TYPEINFO) { }

    public FolderScopeContentTypeExcludedInfo(DataRow dr) : base(TYPEINFO, dr) { }
}
