using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class CloudflareWebGLPostBuild : IPostprocessBuildWithReport
{
    private const int DeployTimeoutMilliseconds = 10 * 60 * 1000;

    public int callbackOrder => 10000;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
        {
            return;
        }

        if (!IsBuildAutomationAgent())
        {
            Debug.Log("[CloudflareWebGLPostBuild] Local/editor WebGL build detected; production Cloudflare deploy skipped.");
            return;
        }

        string branch = Environment.GetEnvironmentVariable("SCM_BRANCH");
        if (!string.Equals(branch, "main", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[CloudflareWebGLPostBuild] UBA branch '{branch ?? "unknown"}' is not production main; deploy skipped.");
            return;
        }

        RequireSecret("CLOUDFLARE_API_TOKEN");
        RequireSecret("CLOUDFLARE_ACCOUNT_ID");

        string projectRoot = Environment.GetEnvironmentVariable("PROJECT_DIRECTORY");
        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        }

        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            throw new BuildFailedException("Unable to resolve project root for production Cloudflare deployment.");
        }

        string scriptPath = Path.Combine(projectRoot, "scripts", "uba-postbuild-cloudflare.sh");
        if (!File.Exists(scriptPath))
        {
            throw new BuildFailedException($"Production deployment script is missing: '{scriptPath}'.");
        }

        string playerPath = Path.GetFullPath(report.summary.outputPath);
        if (!Directory.Exists(playerPath) || !File.Exists(Path.Combine(playerPath, "index.html")))
        {
            throw new BuildFailedException($"WebGL output is not a deployable directory: '{playerPath}'.");
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = Quote(scriptPath),
            WorkingDirectory = projectRoot,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.EnvironmentVariables["UNITY_PLAYER_PATH"] = playerPath;
        startInfo.EnvironmentVariables["PROJECT_DIRECTORY"] = projectRoot;

        Debug.Log($"[CloudflareWebGLPostBuild] Deploying UBA WebGL output '{playerPath}' to production Worker.");

        Process process = Process.Start(startInfo);
        if (process == null)
        {
            throw new BuildFailedException("Failed to launch production Cloudflare deployment process.");
        }

        try
        {
            if (!process.WaitForExit(DeployTimeoutMilliseconds))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Preserve the primary timeout failure below.
                }

                throw new BuildFailedException("Cloudflare production deployment timed out after 10 minutes.");
            }

            if (process.ExitCode != 0)
            {
                throw new BuildFailedException($"Cloudflare production deployment failed with exit code {process.ExitCode}.");
            }
        }
        finally
        {
            process.Dispose();
        }

        Debug.Log("[CloudflareWebGLPostBuild] Production Worker deploy and exact-revision verification completed.");
    }

    private static bool IsBuildAutomationAgent()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable("IS_BUILDER"),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    private static void RequireSecret(string name)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
        {
            throw new BuildFailedException(
                $"Unity Build Automation production target is missing required secret/environment variable '{name}'. " +
                "Refusing to report a successful main WebGL build while the public Worker cannot be updated.");
        }
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
