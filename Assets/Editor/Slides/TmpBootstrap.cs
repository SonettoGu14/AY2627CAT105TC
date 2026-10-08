using UnityEditor;
using UnityEngine;

/// Imports TextMeshPro Essential Resources once, so a fresh clone can build a slide scene
/// without a manual menu step. Idempotent.
///
/// AssetDatabase.ImportPackage is asynchronous in the editor, so resources are NOT on disk
/// yet when this call returns. Ensure() therefore reports "not ready" and finishes later:
/// pass a continuation and it runs as soon as the import has completed and TMP_Settings loads.
public static class TmpBootstrap
{
    // Upper bound on the number of editor frames we wait for the import to become visible.
    const int MaxFrames = 60;

    static System.Action s_OnReady;
    static bool s_Importing;
    static int s_Frames;

    /// Returns true when TMP essentials are ready. When they are not, triggers the
    /// (asynchronous) package import and, if onReady is supplied, invokes it once ready.
    public static bool Ensure(System.Action onReady = null)
    {
        if (IsReady()) return true;

        if (onReady != null) s_OnReady = onReady;

        if (!s_Importing)
        {
            s_Importing = true;
            s_Frames = 0;
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.importPackageCompleted += OnImportCompleted;
            // The bound is anchored here, not to the callback: if importPackageCompleted never
            // fires we must still time out instead of leaving s_Importing stuck true forever.
            EditorApplication.delayCall += PollUntilReady;
        }
        return false;
    }

    static void OnImportCompleted(string packageName)
    {
        if (packageName != "TMP Essential Resources") return;

        AssetDatabase.importPackageCompleted -= OnImportCompleted;
        AssetDatabase.Refresh();
        // PollUntilReady (scheduled when the import was requested) observes readiness and finishes.
    }

    /// Single driver for both outcomes. Runs from Ensure() time regardless of whether the
    /// package callback fired, so the give-up path is always reachable.
    static void PollUntilReady()
    {
        if (!s_Importing) return;   // already finished

        if (IsReady())
        {
            AssetDatabase.importPackageCompleted -= OnImportCompleted;
            s_Importing = false;
            s_Frames = 0;
            Debug.Log("[Slides] imported TMP Essential Resources.");

            System.Action pending = s_OnReady;
            s_OnReady = null;
            if (pending != null) pending();
            return;
        }

        if (++s_Frames >= MaxFrames)
        {
            AssetDatabase.importPackageCompleted -= OnImportCompleted;
            s_Importing = false;
            s_Frames = 0;
            s_OnReady = null;
            Debug.LogError("[Slides] TextMeshPro Essential Resources could not be imported. Run " +
                           "Window > TextMeshPro > Import TMP Essential Resources, then rebuild.");
            return;
        }

        EditorApplication.delayCall += PollUntilReady;
    }

    static bool IsReady()
    {
        try { return TMPro.TMP_Settings.defaultFontAsset != null; }
        catch { return false; }
    }
}
