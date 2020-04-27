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

    bool m_ShowSettings;

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

    private void DoSettings()
    {
        var s = InputSystem.settings;
        //GUILayout.Label("TimesliceEvents: " + s.timesliceEvents.ToString(), Styles.BoldLabel);
        GUILayout.Label("UpdateMode: " + s.updateMode.ToString(), Styles.BoldLabel);
        GUILayout.Label("CompensateForScreenOrientation: " + s.compensateForScreenOrientation.ToString(), Styles.BoldLabel);
        GUILayout.Label("FilterNoiseOnCurrent: " + s.filterNoiseOnCurrent.ToString(), Styles.BoldLabel);
    }

    private void DoSelectDeviceGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Settings", m_ShowSettings ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            m_ShowSettings = !m_ShowSettings;

        foreach (var t in m_DeviceTypes)
        {
            if (GUILayout.Button(t == null ? "All" : t.Name, m_SelectedType == t ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                m_SelectedType = t;
            }
        }
        GUILayout.EndHorizontal();


        if (m_ShowSettings)
        {
            DoSettings();
            return;
        }

        GUILayout.BeginHorizontal();
        m_DeviceScrollView = GUILayout.BeginScrollView(m_DeviceScrollView, GUILayout.Height(10 * Styles.FontSize));
        foreach (var d in InputSystem.devices)
        {
            if (m_SelectedType != null && !m_SelectedType.IsAssignableFrom(d.GetType()))
                continue;

            var name = string.Format("{0} (Type = {1}, Id = {2})", d.displayName, d.GetType().Name, d.deviceId);
            if (GUILayout.Button(name, m_Device != null && m_Device.Device == d ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                if (m_Device != null)
                    m_Device.Dispose();

                if (d as Keyboard != null)
                    m_Device = new KeyboardDevice(d);
                else if (d as Mouse != null)
                    m_Device = new MouseDevice(d);
                else if (d as Touchscreen != null)
                    m_Device = new TouchscreenDevice(d);
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
                GUILayout.Label(string.Format("TouchScreen.current = (Id = {0})", Touchscreen.current != null ? Touchscreen.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
            else if (deviceType == typeof(Keyboard))
                GUILayout.Label(string.Format("Keyboard.current = (Id = {0})", Keyboard.current != null ? Keyboard.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
            else if (deviceType == typeof(Mouse))
                GUILayout.Label(string.Format("Mouse.current = (Id = {0})", Mouse.current != null ? Mouse.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
            else if (deviceType == typeof(Sensor))
                DoSensors();
            else if (deviceType == typeof(Gamepad))
                GUILayout.Label(string.Format("Gamepad.current = (Id = {0})", Gamepad.current != null ? Gamepad.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
            else
                GUILayout.Label(string.Format("Unhandled device type '{0}'", deviceType.GetType().FullName), Styles.BoldLabel);
        }
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }

    private void DoSensors()
    {
        GUILayout.Label(string.Format("Accelerometer.current = (Id = {0})", Accelerometer.current != null ? Accelerometer.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("LightSensor.current = (Id = {0})", LightSensor.current != null ? LightSensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("StepCounter.current = (Id = {0})", StepCounter.current != null ? StepCounter.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("Gyroscope.current = (Id = {0})", UnityEngine.InputSystem.Gyroscope.current != null ? UnityEngine.InputSystem.Gyroscope.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("GravitySensor.current = (Id = {0})", GravitySensor.current != null ? GravitySensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("AttitudeSensor.current = (Id = {0})", AttitudeSensor.current != null ? AttitudeSensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("LinearAccelerationSensor.current = (Id = {0})", LinearAccelerationSensor.current != null ? LinearAccelerationSensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("MagneticFieldSensor.current = (Id = {0})", MagneticFieldSensor.current != null ? MagneticFieldSensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("PressureSensor.current = (Id = {0})", PressureSensor.current != null ? PressureSensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("ProximitySensor.current = (Id = {0})", ProximitySensor.current != null ? ProximitySensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("HumiditySensor.current = (Id = {0})", HumiditySensor.current != null ? HumiditySensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label(string.Format("AmbientTemperatureSensor.current = (Id = {0})", AmbientTemperatureSensor.current != null ? AmbientTemperatureSensor.current.deviceId.ToString() : "<null>"), Styles.BoldLabel); 
    }
}
