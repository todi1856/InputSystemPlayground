using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EntryPoint : MonoBehaviour
{
    Devices m_Devices;
    Touchscreen m_FakeTouchScreen;

    // Start is called before the first frame update
    void Start()
    {
        m_Devices = new Devices();
        bool injectFakeTouchscreen = Configuration.Instance.OldInputEnabled;
#if UNITY_EDITOR
        injectFakeTouchscreen = true;
#endif
        if (injectFakeTouchscreen)
            m_FakeTouchScreen = InputSystem.AddDevice<Touchscreen>("Fake Touchscreen");
    }

    private void OnDisable()
    {
        if (m_FakeTouchScreen != null)
        {
            InputSystem.RemoveDevice(m_FakeTouchScreen);
            m_FakeTouchScreen = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_Devices.DoUpdate();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Size: " + Styles.FontSize);
        Styles.FontSize = GUILayout.HorizontalScrollbar(Styles.FontSize, 1, 11.0f, 60.0f, Styles.HorizontalScrollbar);
        GUILayout.EndHorizontal();
        m_Devices.DoGUI();
    }
}
