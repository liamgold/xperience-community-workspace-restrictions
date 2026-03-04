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
        InstallBindingClass(resource);
        InstallExclusionClass(resource);
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

    private static void InstallBindingClass(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(WorkspaceContentTypeBindingInfo.TYPEINFO.ObjectClassName)
            ?? DataClassInfo.New(WorkspaceContentTypeBindingInfo.OBJECT_TYPE);

        info.ClassName = WorkspaceContentTypeBindingInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = WorkspaceContentTypeBindingInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Workspace Content Type Binding";
        info.ClassResourceID = resource.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(WorkspaceContentTypeBindingInfo.WorkspaceContentTypeBindingID));

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeBindingInfo.WorkspaceContentTypeBindingWorkspaceID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        });

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeBindingInfo.WorkspaceContentTypeBindingClassID),
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

    private static void InstallExclusionClass(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(WorkspaceContentTypeExclusionInfo.TYPEINFO.ObjectClassName)
            ?? DataClassInfo.New(WorkspaceContentTypeExclusionInfo.OBJECT_TYPE);

        info.ClassName = WorkspaceContentTypeExclusionInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = WorkspaceContentTypeExclusionInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Workspace Content Type Exclusion";
        info.ClassResourceID = resource.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(WorkspaceContentTypeExclusionInfo.WorkspaceContentTypeExclusionID));

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeExclusionInfo.WorkspaceContentTypeExclusionWorkspaceID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        });

        formInfo.AddFormItem(new FormFieldInfo
        {
            Name = nameof(WorkspaceContentTypeExclusionInfo.WorkspaceContentTypeExclusionClassID),
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
