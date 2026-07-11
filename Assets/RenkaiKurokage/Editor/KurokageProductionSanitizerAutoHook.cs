/// <summary>
/// Intentionally inert.
///
/// Production preparation is owned by:
/// - Renkai/Build Production Version
/// - KurokageProductionBuildPreprocessor
///
/// The former InitializeOnLoad scene hook could race with the transition into
/// Play Mode and call EditorSceneManager.MarkSceneDirty while the game was
/// already running. Keeping this type as a compatibility stub avoids stale
/// script references without performing destructive editor work automatically.
/// </summary>
public static class KurokageProductionSanitizerAutoHook
{
}
