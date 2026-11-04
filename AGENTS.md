# Xperience Community: Workspace Restrictions

A NuGet package that extends the Xperience by Kentico administration. It restricts which reusable
content types editors can create in a workspace or in one of its folders.

- Keep the package zero-configuration. Register everything with assembly attributes
  (`RegisterModule`, `UIPage`, `PageExtender`, `RegisterObjectType`). Don't add services or
  startup code that a site would have to call.
- The package has no migrations. `WorkspaceRestrictionsInstaller` creates and updates the tables
  every time the application starts. Keep it idempotent, and never let it delete or merge existing
  data.
- The package filters the content type picker on the create page
  (`WorkspaceContentTypeFilterExtender`) and blocks Content hub moves during web requests
  (`ContentItemMoveGuard`). Both use `ContentTypeRestrictionResolver`, which stays synchronous
  because the guard runs inside Kentico's move transaction. The guard depends on how Kentico's
  internal move code saves items, so test single and mass moves after an Xperience upgrade. Kentico
  shows its own error message when a move fails, so `src/Localization` replaces the English text
  of those two resource keys. Check after an upgrade that the keys still exist. Nothing
  checks items that are created or imported through the API. Issue #6 tracks this gap.
- The SQL Server tests in `tests/FolderScopeDatabaseTests.cs` are skipped unless
  `WORKSPACE_RESTRICTIONS_TEST_CONNECTION_STRING` is set, and the test run still passes. Set the
  variable before you change the installer or `WorkspaceFolderScopePersistence`. The README shows
  how.
- Test admin UI changes by hand in `examples/DancingGoat`, as the README describes. The example is
  generated Kentico template code with its own `AGENTS.md`. Change only how it uses this library.
  CI doesn't build it, and CodeQL skips it.
- To raise the minimum Xperience version, use the `update-xperience-version` skill.
