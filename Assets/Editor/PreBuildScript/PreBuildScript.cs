using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PreBuildScript : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    static PreBuildScript()
    {
        // Register event handlers for play mode state changes
        // MobileUO: TODO: does not seem to work unless you restart Unity - commenting out for now
        //EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        // Check if the build process is coming from GitHub Actions
        bool isGitHubActions = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION"));
        Debug.Log($"Is GitHub Actions build: {isGitHubActions}");

        // Apply settings at build time
        ApplySettingsFromFile(report.summary.platform, isGitHubActions);
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Apply settings when entering play mode
            ApplySettingsFromFile(EditorUserBuildSettings.activeBuildTarget);
        }
    }

    private static void ApplySettingsFromFile(BuildTarget platform, bool isGitHubActionsBuild = false)
    {
        if (platform == BuildTarget.Android)
        {
            string environment = (EditorUserBuildSettings.development || isGitHubActionsBuild) ? "Development" : "Production";
            string jsonFileName = $"appsettings.{environment}.json";

            Debug.Log($"Environment: {environment}");
            Debug.Log($"Reading settings from: {jsonFileName}");

            string jsonPath = Path.Combine(Application.dataPath, jsonFileName);
            Debug.Log($"Appsettings path: {jsonPath}");

            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                AppSettings settings = JsonUtility.FromJson<AppSettings>(jsonContent);

                Debug.Log($"Applying settings: AndroidPackageName = {settings.AndroidPackageName}, ProductName = {settings.ProductName}, AndroidUseCustomKeystore = {settings.AndroidUseCustomKeystore}");

                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, settings.AndroidPackageName);
                PlayerSettings.productName = settings.ProductName;
                PlayerSettings.Android.useCustomKeystore = settings.AndroidUseCustomKeystore;
            }
            else
            {
                Debug.LogWarning($"{jsonFileName} file not found.");
            }
        }
    }

    [System.Serializable]
    private class AppSettings
    {
        public string AndroidPackageName;
        public string ProductName;
        public bool AndroidUseCustomKeystore;
    }
}