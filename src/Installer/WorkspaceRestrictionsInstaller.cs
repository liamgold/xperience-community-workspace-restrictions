using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace XperienceCommunity.WorkspaceRestrictions;

internal class WorkspaceRestrictionsInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider)
{
    private const string ResourceName = "XperienceCommunity.WorkspaceRestrictions";
    private const string ResourceDisplayName = "Workspace Restrictions";

    public void Install()
    {
        var resource = InstallResource();
        InstallAllowedClass(resource);
        InstallExcludedClass(resource);
        InstallFolderScopeClass(resource);
        InstallFolderScopeContentTypeClass(resource, FolderScopeContentTypeAllowedInfo.TYPEINFO,
            "Folder Scope Content Type Allowed",
            nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedID),
            nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedFolderScopeID),
            nameof(FolderScopeContentTypeAllowedInfo.FolderScopeContentTypeAllowedClassID));
        InstallFolderScopeContentTypeClass(resource, FolderScopeContentTypeExcludedInfo.TYPEINFO,
            "Folder Scope Content Type Excluded",
            nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedID),
            nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedFolderScopeID),
            nameof(FolderScopeContentTypeExcludedInfo.FolderScopeContentTypeExcludedClassID));
    }

    private ResourceInfo InstallResource()
    {
        var resource = resourceInfoProvider.Get(ResourceName) ?? new ResourceInfo();

        resource.ResourceDisplayName = ResourceDisplayName;
        resource.ResourceName = ResourceName;
        resource.ResourceIsInDevelopment = false;

        if (resource.HasChanged)
        {
            resourceInfoProvider.Set(resource);
        }

        return resource;
    }

    private static void InstallAllowedClass(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(WorkspaceContentTypeAllowedInfo.TYPEINFO.ObjectClassName)
            ?? DataClassInfo.New(WorkspaceContentTypeAllowedInfo.OBJECT_TYPE);

        info.ClassName = WorkspaceContentTypeAllowedInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = WorkspaceContentTypeAllowedInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Workspace Content Type Allowed";
        info.ClassResourceID = resource.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedID));

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedWorkspaceID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        });

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeAllowedInfo.WorkspaceContentTypeAllowedClassID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        });

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    private static void InstallExcludedClass(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(WorkspaceContentTypeExcludedInfo.TYPEINFO.ObjectClassName)
            ?? DataClassInfo.New(WorkspaceContentTypeExcludedInfo.OBJECT_TYPE);

        info.ClassName = WorkspaceContentTypeExcludedInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = WorkspaceContentTypeExcludedInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Workspace Content Type Excluded";
        info.ClassResourceID = resource.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(WorkspaceContentTypeExcludedInfo.WorkspaceContentTypeExcludedID));

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeExcludedInfo.WorkspaceContentTypeExcludedWorkspaceID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        });

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeExcludedInfo.WorkspaceContentTypeExcludedClassID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        });

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    private static void InstallFolderScopeClass(ResourceInfo resource)
    {
        var formInfo = FormHelper.GetBasicFormDefinition(nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeID));

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeGUID),
            Visible = false,
            DataType = FieldDataType.Guid,
            Enabled = true,
        });

        AddIntegerField(formInfo, nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID));
        AddIntegerField(formInfo, nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID));

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeApplyToChildFolders),
            Visible = false,
            DataType = FieldDataType.Boolean,
            DefaultValue = "false",
            Enabled = true,
        });

        InstallClass(resource, WorkspaceFolderScopeInfo.TYPEINFO, "Workspace Folder Scope", formInfo);
        InstallFolderScopeIndex(DataConnectionFactory.TableManagerObject);
    }

    internal static void InstallFolderScopeIndex(ITableManager tableManager) => tableManager.CreateIndex(
        WorkspaceFolderScopePersistence.UniqueFolderIndexName,
        WorkspaceFolderScopeInfo.TYPEINFO.ObjectClassName.Replace(".", "_"),
        isUnique: true,
        columns:
        [
            (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeWorkspaceID), true),
            (nameof(WorkspaceFolderScopeInfo.WorkspaceFolderScopeFolderID), true),
        ]);

    private static void InstallFolderScopeContentTypeClass(ResourceInfo resource, ObjectTypeInfo typeInfo,
        string displayName, string idColumn, string scopeColumn, string classColumn)
    {
        var formInfo = FormHelper.GetBasicFormDefinition(idColumn);
        AddIntegerField(formInfo, scopeColumn);
        AddIntegerField(formInfo, classColumn);

        InstallClass(resource, typeInfo, displayName, formInfo);
    }

    private static void AddIntegerField(FormInfo formInfo, string name) => formInfo.AddFormItem(new FormFieldInfo
    {
        Name = name,
        Visible = false,
        DataType = FieldDataType.Integer,
        Enabled = true,
    });

    private static void InstallClass(ResourceInfo resource, ObjectTypeInfo typeInfo, string displayName, FormInfo formInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(typeInfo.ObjectClassName)
            ?? DataClassInfo.New(typeInfo.ObjectType);

        info.ClassName = typeInfo.ObjectClassName;
        info.ClassTableName = typeInfo.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = displayName;
        info.ClassResourceID = resource.ResourceID;
        info.ClassType = ClassType.OTHER;

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
