using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

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
                var propNew = playerSettingsSo.FindProperty("enableNativePlatformBackendsForNewInputSystem");
                var propOld = playerSettingsSo.FindProperty("disableOldInputManagerSupport");
                m_Instance.NewInputEnabled = propNew.boolValue;
                m_Instance.OldInputEnabled = propOld.boolValue == false;
#else  
                if (File.Exists(ConfigurationPath) == false)
                    throw new Exception("Config not found: " + ConfigurationPath);
                var contents = File.ReadAllText(ConfigurationPath);
                m_Instance = JsonUtility.FromJson<Configuration>(contents);
#endif
            }

            return m_Instance;
        }
    }

}
