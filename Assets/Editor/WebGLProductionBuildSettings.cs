using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Production WebGL publishing settings shared by every remote build path.
/// Keeps the generated Unity payload small enough for the public Cloudflare Worker
/// and includes a browser-side decompression fallback so hosting headers cannot
/// silently make the player unloadable.
/// </summary>
public sealed class WebGLProductionBuildSettings : IPreprocessBuildWithReport
{
    public int callbackOrder => -10000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
        {
            return;
        }

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.showDiagnostics = false;

        Debug.Log("[WebGLProductionBuildSettings] Brotli compression, decompression fallback and data caching enabled.");
    }
}
