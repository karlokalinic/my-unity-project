using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KarloLegend
{
    public static class AndroidV16Build
    {
        private const string ProductName = "ZRAKOPERKA";
        private const string ApplicationId = "com.karlolegend.zrakoperka";
        private const string VersionName = "0.16.0";
        private const int VersionCode = 16;
        private const string OutputPath = "build/Android/ZRAKOPERKA-v16.apk";

        public static void Build()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationId);
            EditorUserBuildSettings.buildAppBundle = false;

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");

            Directory.CreateDirectory("build/Android");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Android v16 build failed: {summary.result}; errors={summary.totalErrors}; warnings={summary.totalWarnings}");

            if (!File.Exists(OutputPath))
                throw new BuildFailedException($"Unity reported success but APK was not found at {OutputPath}.");

            Debug.Log($"[AndroidV16Build] APK ready: {OutputPath} ({summary.totalSize} bytes)");
        }
    }
}
