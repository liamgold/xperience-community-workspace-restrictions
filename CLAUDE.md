# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build --configuration Release

# Restore dependencies
dotnet restore

# Pack NuGet package
dotnet pack --configuration Release
```

There are currently no tests. The CI pipeline runs `dotnet test` with `continue-on-error: true`.

## Architecture

This is a single-project NuGet library (`src/XperienceCommunity.WorkspaceRestrictions.csproj`) targeting `net10.0` that extends the Xperience by Kentico (XbK) admin UI. It has no service registration step — the module self-registers via `[assembly: CMS.RegisterModule]`.

### How it works

The library adds two tabs ("Allowed content types" and "Excluded content types") to the Workspace edit page in the XbK admin, and filters the content type picker when creating content items.

**Key extension points (all registered via assembly attributes, no DI registration needed):**

| File | Mechanism | Purpose |
|---|---|---|
| `WorkspaceRestrictionsModule.cs` | `[RegisterModule]` | Entry point; runs `WorkspaceContentTypeBindingInstaller` on app start to create DB tables |
| `WorkspaceEditSectionExtender.cs` | `[PageExtender]` on `WorkspaceEditSection` | Injects the two nav tabs into the workspace edit page |
| `WorkspaceContentTypeBindingPage.cs` | `[UIPage]` under `WorkspaceEditSection` | Allow-list UI tab using `InfoBindingPage<WorkspaceContentTypeBindingInfo, DataClassInfo>` |
| `WorkspaceContentTypeExclusionPage.cs` | `[UIPage]` under `WorkspaceEditSection` | Exclude-list UI tab (same pattern) |
| `WorkspaceContentTypeFilterExtender.cs` | `[PageExtender]` on `ContentItemCreate` | Filters `TileSelectorClientProperties.Items` based on the allow/exclude lists at runtime |

**Data model — two binding Info objects:**
- `WorkspaceContentTypeBindingInfo` (allow list): `OBJECT_TYPE = "xperiencecommunity.workspacecontenttypebinding"`, columns: ID, WorkspaceID, ClassID
- `WorkspaceContentTypeExclusionInfo` (exclude list): same shape, different object type

Both are installed programmatically by `WorkspaceContentTypeBindingInstaller` — no migration files or CI/CD schema steps required. The installer is idempotent (checks `HasChanged` before saving).

**Filtering logic** (in `WorkspaceContentTypeFilterExtender`): allow list takes precedence — if any allowed bindings exist for the workspace, the exclude list is ignored entirely.

### Package versioning

Package versions are managed centrally in `Directory.Packages.props`. The `Kentico.Xperience.Admin` package version determines the minimum compatible XbK version (currently `31.2.1`). When bumping the XbK dependency, update `Directory.Packages.props` and the compatibility table in `README.md`.
