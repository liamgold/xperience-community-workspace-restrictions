# Xperience Community: Workspace Restrictions

A NuGet package that extends the Xperience by Kentico administration. It restricts which reusable
content types editors can create in a workspace or in one of its folders.

- Keep the package zero-configuration. Register everything with assembly attributes
  (`RegisterModule`, `UIPage`, `PageExtender`, `RegisterObjectType`). Don't add services or
  startup code that a site would have to call.
- The package has no migrations. `WorkspaceRestrictionsInstaller` creates and updates the tables
  every time the application starts. Keep it idempotent, and never let it delete or merge existing
  data.
- The package applies restrictions only by filtering the content type picker on the create page
  (`WorkspaceContentTypeFilterExtender`). Nothing checks the content type when an item is saved,
  moved or imported. Issue #6 tracks this gap.
- The SQL Server tests in `tests/FolderScopeDatabaseTests.cs` are skipped unless
  `WORKSPACE_RESTRICTIONS_TEST_CONNECTION_STRING` is set, and the test run still passes. Set the
  variable before you change the installer or `WorkspaceFolderScopePersistence`. The README shows
  how.
- Test admin UI changes by hand in `examples/DancingGoat`, as the README describes. The example is
  generated Kentico template code with its own `AGENTS.md`. Change only how it uses this library.
  CI doesn't build it, and CodeQL skips it.
- To raise the minimum Xperience version, use the `update-xperience-version` skill.
