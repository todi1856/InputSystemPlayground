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
            GUILayout.Button("No devices", Styles.BoldButton);
            return;
        }
        GUILayout.BeginHorizontal();
        foreach (var d in InputSystem.devices)
        {
            if (GUILayout.Button(d.displayName, m_Device != null && m_Device.Device == d ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                if (m_Device != null)
                    m_Device.Dispose();

                if (d as Keyboard != null)
                    m_Device = new KeyboardDevice(d);
                else if (d as Mouse != null)
                    m_Device = new MouseDevice(d);
                else if (d as Sensor != null)
                    m_Device = new SensorDevice(d);
                else
                    m_Device = new GenericDevice(d);
            }
        }
        GUILayout.EndHorizontal();

        if (m_Device != null)
            m_Device.DoGUI();
    }
}
