using System.IO;
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
    static System.Action s_OnReady;
    static bool s_Importing;
    static int s_Retries;

    /// Returns true when TMP essentials are ready. When they are not, triggers the
    /// (asynchronous) package import and, if onReady is supplied, invokes it once ready.
    public static bool Ensure(System.Action onReady = null)
    {
        if (IsReady()) return true;

        if (onReady != null) s_OnReady = onReady;

        if (!s_Importing)
        {
            s_Importing = true;
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.importPackageCompleted += OnImportCompleted;
        }
        return false;
    }

    static void OnImportCompleted(string packageName)
    {
        // Only the essentials matter; the callback can fire for other packages too.
        if (packageName != "TMP Essential Resources") return;

        AssetDatabase.importPackageCompleted -= OnImportCompleted;
        AssetDatabase.Refresh();
        FinishWhenReady();
    }

    static void FinishWhenReady()
    {
        if (!IsReady())
        {
            // The asset database may need another frame to expose the freshly imported
            // Resources folder. Retry a bounded number of frames, then give up cleanly.
            if (++s_Retries < 60) { EditorApplication.delayCall += FinishWhenReady; return; }

            s_Retries = 0;
            s_Importing = false;
            Debug.LogError("[Slides] TextMeshPro Essential Resources could not be imported. Run " +
                           "Window > TextMeshPro > Import TMP Essential Resources, then rebuild.");
            return;
        }

        s_Retries = 0;
        s_Importing = false;
        Debug.Log("[Slides] imported TMP Essential Resources.");

        System.Action pending = s_OnReady;
        s_OnReady = null;
        if (pending != null) pending();
    }

    static bool IsReady()
    {
        try { return TMPro.TMP_Settings.defaultFontAsset != null; }
        catch { return false; }
    }
}
