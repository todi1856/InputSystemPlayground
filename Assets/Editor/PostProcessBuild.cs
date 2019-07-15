using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;
using UnityEditor.Build.Reporting;


class MyCustomBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "StreamingAssets"));
        File.WriteAllText(Configuration.ConfigurationPath, JsonUtility.ToJson(Configuration.Instance));
    }
}