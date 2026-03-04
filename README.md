# Xperience Community: Workspace Restrictions

[![NuGet](https://img.shields.io/nuget/v/XperienceCommunity.WorkspaceRestrictions)](https://www.nuget.org/packages/XperienceCommunity.WorkspaceRestrictions)

Restrict which reusable content types are available when creating content items in a workspace in [Xperience by Kentico](https://www.kentico.com/).

## Features

- **Allow list**: Specify which reusable content types are available in a workspace. When configured, only the listed content types will appear in the "Create content item" dialog for that workspace.
- **Exclude list**: Specify which reusable content types are hidden in a workspace. When configured, all content types except the listed ones will appear in the "Create content item" dialog.

> Allow list takes precedence over the exclude list. If any allowed content types are configured, the exclude list is ignored for that workspace.

## Requirements

| Xperience by Kentico version | Package version |
|------------------------------|-----------------|
| >= 31.2.1                    | >= 1.0.0        |

## Installation

Install the NuGet package in your Xperience by Kentico admin project:

```shell
dotnet add package XperienceCommunity.WorkspaceRestrictions
```

No additional configuration is required. The package self-installs its database tables on application startup.

## Usage

1. Navigate to **Content hub** → **Configuration** → **Workspaces** in the Xperience administration.
2. Open the workspace you want to configure.
3. Use the **Allowed content types** tab to specify an allow list, or the **Excluded content types** tab to specify an exclude list.
4. Changes take effect immediately — no restart required.

### Configuring allowed content types

Use the **Allowed content types** tab to build an allow list for the workspace. Only the listed content types will appear when creating content items in this workspace.

<a href="/src/images/AllowedContentTypes.png">
  <img src="/src/images/AllowedContentTypes.png" width="800" alt="Allowed content types tab on a workspace in Xperience by Kentico">
</a>

### Configuring excluded content types

Use the **Excluded content types** tab to build an exclude list. All content types except the listed ones will appear when creating content items in this workspace.

<a href="/src/images/ExcludedContentTypes.png">
  <img src="/src/images/ExcludedContentTypes.png" width="800" alt="Excluded content types tab on a workspace in Xperience by Kentico">
</a>

### Filtered content type picker

When creating a content item, the content type picker is automatically filtered based on the workspace configuration.

<a href="/src/images/FilteredContentTypePicker.png">
  <img src="/src/images/FilteredContentTypePicker.png" width="800" alt="Filtered content type picker when creating a content item in Xperience by Kentico">
</a>

## Contributing

Contributions are welcome! Please open an issue or pull request on [GitHub](https://github.com/liamgold/xperience-community-workspace-restrictions).

## License

This project is licensed under the [MIT License](LICENSE.md).
