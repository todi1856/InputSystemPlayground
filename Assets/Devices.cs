using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Devices
{
    GenericDevice m_Device;

    public void DoGUI()
    {
        if (InputSystem.devices.Count == 0)
        {
            GUILayout.Button("No devices");
            return;
        }
        GUILayout.BeginHorizontal();
        foreach (var d in InputSystem.devices)
        {
            if (GUILayout.Button(d.displayName))
            {
                if (m_Device != null)
                    m_Device.Dispose();
                m_Device = new GenericDevice(d);
            }
        }
        GUILayout.EndHorizontal();

        if (m_Device != null)
            m_Device.DoGUI();
    }
}
