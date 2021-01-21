using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class Configuration
{
    private static Configuration m_Instance;
    public bool OldInputEnabled;

    public bool NewInputEnabled;

    public static string ConfigurationPath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "StreamingAssets/config.ini");
#elif UNITY_ANDROID
            return "jar:file://" + Application.dataPath + "!/assets/config.ini";
#elif UNITY_IOS
            return Application.dataPath + "/Raw/config.ini";
#else
            return Path.Combine(Application.dataPath, "StreamingAssets/config.ini");
#endif
        }
    }

    public static Configuration Instance
    {
        get
        {
            if (m_Instance == null)
            {
#if UNITY_EDITOR
                m_Instance = new Configuration();
                PlayerSettings playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>()[0];
                SerializedObject playerSettingsSo = new SerializedObject(playerSettings);
#if UNITY_2020_2_OR_NEWER
                var activeInputHandler = playerSettingsSo.FindProperty("activeInputHandler").intValue;
                m_Instance.NewInputEnabled = activeInputHandler == 1 || activeInputHandler == 2;
                m_Instance.OldInputEnabled = activeInputHandler == 0 || activeInputHandler == 2; ;

#else
                var propNew = playerSettingsSo.FindProperty("enableNativePlatformBackendsForNewInputSystem");
                var propOld = playerSettingsSo.FindProperty("disableOldInputManagerSupport");
                m_Instance.NewInputEnabled = propNew.boolValue;
                m_Instance.OldInputEnabled = propOld.boolValue == false;
#endif

#else
                var uwr = UnityWebRequest.Get(ConfigurationPath);
                uwr.SendWebRequest();
                while (!uwr.isDone && !uwr.isNetworkError && !uwr.isHttpError)
                {
                    Debug.Log("Downloading");
                    Thread.Sleep(10);
                }
                var contents = uwr.downloadHandler.text;
                m_Instance = JsonUtility.FromJson<Configuration>(contents);
#endif

            }

            return m_Instance;
        }
    }

}
