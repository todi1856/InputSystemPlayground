using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Devices
{
    GenericDevice m_Device;
    Vector2 m_DeviceScrollView;
    readonly Type[] m_DeviceTypes = new[] { null, typeof(Touchscreen), typeof(Mouse), typeof(Keyboard), typeof(Gamepad), typeof(Sensor) };
    Type m_SelectedType;

    public void DoUpdate()
    {
        if (m_Device != null)
            m_Device.DoUpdate();
    }

    public void DoGUI()
    {
        if (InputSystem.devices.Count == 0)
        {
            GUILayout.Button("No devices", Styles.BoldButton);
            return;
        }
        if (m_Device == null)
        {
            DoSelectDeviceGUI();
        }
        else
        {
            if (GUILayout.Button("Select different device", Styles.BoldButton))
            {
                m_Device.Dispose();
                m_Device = null;
            }
            GUILayout.Space(10);
            if (m_Device != null)
                m_Device.DoGUI();
        }
    }

    private void DoSelectDeviceGUI()
    {
        GUILayout.BeginHorizontal();
        foreach (var t in m_DeviceTypes)
        {
            if (GUILayout.Button(t == null ? "All" : t.Name, m_SelectedType == t ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                m_SelectedType = t;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        m_DeviceScrollView = GUILayout.BeginScrollView(m_DeviceScrollView, GUILayout.Height(10 * Styles.FontSize));
        foreach (var d in InputSystem.devices)
        {
            if (m_SelectedType != null && !d.GetType().IsAssignableFrom(m_SelectedType))
                continue;

            var name = string.Format("{0} (Type = {1}, Id = {2})", d.displayName, d.GetType().Name, d.id);
            if (GUILayout.Button(name, m_Device != null && m_Device.Device == d ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                if (m_Device != null)
                    m_Device.Dispose();

                if (d as Keyboard != null)
                    m_Device = new KeyboardDevice(d);
                else if (d as Mouse != null)
                    m_Device = new MouseDevice(d);
                else if (d as Sensor != null)
                    m_Device = new SensorDevice(d);
                else if (d as Gamepad != null)
                    m_Device = new GamepadDevice(d);
                else
                    m_Device = new GenericDevice(d);
            }
        }

        foreach (var deviceType in m_DeviceTypes)
        {
            if (deviceType == null || m_SelectedType != deviceType)
                continue;

            if (deviceType == typeof(Touchscreen))
                GUILayout.Label(string.Format("TouchScreen.current = (Id = {0})", Touchscreen.current != null ? Touchscreen.current.id.ToString() : "<null>"), Styles.BoldLabel);
            else if (deviceType == typeof(Keyboard))
                GUILayout.Label(string.Format("Keyboard.current = (Id = {0})", Keyboard.current != null ? Keyboard.current.id.ToString() : "<null>"), Styles.BoldLabel);
            else if(deviceType == typeof(Mouse))
                GUILayout.Label(string.Format("Mouse.current = (Id = {0})", Mouse.current != null ? Mouse.current.id.ToString() : "<null>"), Styles.BoldLabel);
            else if(deviceType == typeof(Sensor))
                GUILayout.Label(string.Format("ToDo sensor"), Styles.BoldLabel);
            else if (deviceType == typeof(Gamepad))
                GUILayout.Label(string.Format("Gamepad.current = (Id = {0})", Gamepad.current != null ? Gamepad.current.id.ToString() : "<null>"), Styles.BoldLabel);
            else
                GUILayout.Label(string.Format("Unhandled device type '{0}'", deviceType.GetType().FullName), Styles.BoldLabel);
        }
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }
}
