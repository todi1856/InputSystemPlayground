using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    Devices m_Devices;
    // Start is called before the first frame update
    void Start()
    {
        m_Devices = new Devices();
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
