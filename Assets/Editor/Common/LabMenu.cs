using UnityEditor;
using UnityEngine;

/// <summary>
/// Convenience entry points that span more than one week.
///
/// Per-week building lives with each week: CAT105TC &gt; W03 Physics2D &gt; ... ,
/// CAT105TC &gt; W04 Animation &amp; Camera &gt; ... . This file only adds the
/// cross-week shortcuts and the shared-asset refresh.
/// </summary>
public static class LabMenu
{
    [MenuItem("CAT105TC/Common/Rebuild shared assets (layers, tags, sprites, materials)")]
    public static void RebuildSharedAssets()
    {
        LabKit.SetupSharedAssets();
    }

    [MenuItem("CAT105TC/Build all current playgrounds")]
    public static void BuildAll()
    {
        W04PlaygroundBuilder.Build();
        W03PlaygroundBuilder.Build();
        Debug.Log("[LabMenu] all labs built.");
    }
}
