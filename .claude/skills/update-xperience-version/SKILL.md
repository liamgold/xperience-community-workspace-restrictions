---
name: update-xperience-version
description: Raise the minimum Xperience by Kentico version this package supports. Use when bumping the Kentico.Xperience.* packages.
---

# Update the Xperience version

The `Kentico.Xperience.Admin` version in `Directory.Packages.props` sets the minimum Xperience
version that sites need to install this package. To raise it, follow these steps.

1. Update every `Kentico.Xperience.*` version in `Directory.Packages.props` to the new version.
   The example site uses the other Kentico packages, so keep them all on the same version.
2. Update the `kentico.xperience.dbmanager` version in
   `examples/DancingGoat/.config/dotnet-tools.json` to the same version.
3. Check whether the example site still runs against a new database. If the sample data or the
   template code changed, regenerate the example from the matching `Kentico.Xperience.Templates`
   version. Then add back the project reference to `src`, the central package management setup
   and the csproj overrides. Commit `36bc73d` added the example, and shows what to keep.
4. Regenerate the lock files. Run `dotnet restore --force-evaluate` at the repo root and in
   `examples/DancingGoat`. This updates `src/packages.lock.json`, `tests/packages.lock.json` and
   `examples/DancingGoat/packages.lock.json`.
5. Raise the major version in `src/XperienceCommunity.WorkspaceRestrictions.csproj`. Sites on the
   old minimum can't install the new release, so treat the change as breaking.
6. Add a row for the new versions at the top of the "Library Version Matrix" table in `README.md`.
   Also update the template version named in the "Running the Example Project" section.
7. Build and run the tests. Set `WORKSPACE_RESTRICTIONS_TEST_CONNECTION_STRING` so that the SQL
   Server tests run too.
8. Check the internal API. `WorkspaceFolderScopeCreatePage` and `WorkspaceFolderScopeSettingsPage`
   use `ContentFolderSelectorComponent` from `Kentico.Xperience.Admin.Base.Forms.Internal`.
   Kentico doesn't keep internal APIs stable between versions, so confirm that the code builds and
   that the folder picker works.
9. Run the example site, and test the workspace tabs, the folder scopes and the filtered content
   type picker in the administration.
