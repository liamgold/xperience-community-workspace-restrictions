using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace XperienceCommunity.WorkspaceRestrictions;

internal class WorkspaceContentTypeBindingInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider)
{
    private const string ResourceName = "XperienceCommunity.WorkspaceRestrictions";
    private const string ResourceDisplayName = "Workspace Restrictions";

    public void Install()
    {
        var resource = InstallResource();
        InstallAllowedClass(resource);
        InstallExcludedClass(resource);
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
