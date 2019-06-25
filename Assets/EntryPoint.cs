using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    float m_Scale = 1.0f;
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
        GUI.matrix = Matrix4x4.Scale(Vector3.one * m_Scale);
        m_Scale = GUILayout.HorizontalSlider(m_Scale, 1.0f, 4.0f);
        m_Devices.DoGUI();
    }
}
