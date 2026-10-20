using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Build;

public static class PlatformZeroPackageSetup
{
    private static AddRequest request;
    private static double deadline;
    private static bool quitWhenDone;

    [InitializeOnLoadMethod]
    private static void AutoSetup()
    {
        if (!Application.isBatchMode) EditorApplication.delayCall += CheckPackage;
    }

    private static void CheckPackage()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += CheckPackage; return; }
        if (UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p => p.name == "dev.yarnspinner.unity")) EnableGame();
        else if (!SessionState.GetBool("PlatformZero.InstallAttempted", false)) Install();
    }

    private static void EnableGame()
    {
        var target = NamedBuildTarget.Standalone;
        var symbols = PlayerSettings.GetScriptingDefineSymbols(target).Split(';').Where(s => !string.IsNullOrEmpty(s)).ToList();
        if (symbols.Contains("PLATFORM_ZERO_YARN")) return;
        symbols.Add("PLATFORM_ZERO_YARN");
        PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
    }

    [MenuItem("Platform Zero/Install Yarn Spinner 3.2")]
    public static void Install()
    {
        if (request != null) return;
        quitWhenDone = Application.isBatchMode;
        SessionState.SetBool("PlatformZero.InstallAttempted", true);
        request = Client.Add("file:../LocalPackages~/YarnSpinner-3.2.0.tgz");
        deadline = EditorApplication.timeSinceStartup + 600;
        EditorApplication.update += Poll;
    }

    private static void Poll()
    {
        if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
        EditorApplication.update -= Poll;
        bool success = request.IsCompleted && request.Status == StatusCode.Success;
        if (success) { Debug.Log("Platform Zero: Yarn Spinner installed: " + request.Result.version); EnableGame(); }
        else Debug.LogError("Platform Zero: Yarn installation failed: " + request.Error?.message);
        request = null;
        if (quitWhenDone) EditorApplication.Exit(success ? 0 : 1);
    }
}
