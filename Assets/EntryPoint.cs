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
        
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Size: " + Styles.FontSize);
        Styles.FontSize = GUILayout.HorizontalSlider(Styles.FontSize, 11.0f, 30.0f, GUILayout.Width(600));
        GUILayout.EndHorizontal();
        m_Devices.DoGUI();
    }
}
