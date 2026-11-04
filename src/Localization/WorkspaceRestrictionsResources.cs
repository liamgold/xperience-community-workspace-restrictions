using CMS.Base;
using CMS.Localization;
using XperienceCommunity.WorkspaceRestrictions.Localization;

// Replaces Kentico's English "move failed" messages, so editors know that a restriction might have blocked the move.
// ContentItemMoveGuard can't change the message itself, because Kentico catches its exception.
[assembly: RegisterLocalizationResource(typeof(WorkspaceRestrictionsResources), LocalizationTarget.Server, SystemContext.SYSTEM_CULTURE_NAME)]

namespace XperienceCommunity.WorkspaceRestrictions.Localization;

internal sealed class WorkspaceRestrictionsResources
{
}
