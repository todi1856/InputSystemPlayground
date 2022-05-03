using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public partial class Devices
{
    GenericDevice m_Device;
    Vector2 m_DeviceScrollView;
    readonly Type[] m_DeviceTypes = new[] { null, typeof(Touchscreen), typeof(Mouse), typeof(Keyboard), typeof(Pen), typeof(Gamepad), typeof(Sensor) };
    Type m_SelectedType;

    bool m_ShowSettings;

    public Devices()
    {
        EnhancedTouchSupport.Enable();
    }

    public void DoUpdate()
    {
        if (m_Device != null)
            m_Device.DoUpdate();

        foreach (var deviceType in m_DeviceTypes)
        {
            if (deviceType == null || m_SelectedType != deviceType)
                continue;

            if (deviceType == typeof(Touchscreen))
                DoGenericTouchScreenUpdate();
        }
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

        foreach (var deviceType in m_DeviceTypes)
        {
            if (deviceType == null || m_SelectedType != deviceType)
                continue;

            if (deviceType == typeof(Touchscreen))
                DoGenericTouchScreenGUI();
        }
    }


    private string GetDeviceInfo(InputDevice device)
    {
        bool enabled = false;
        if (device != null)
            enabled = device.enabled;
        return string.Format("(Id = {0}, Enabled = {1})", device != null ? device.deviceId.ToString() : " <null>", enabled);
    }

    private void DoSettings()
    {
        var s = InputSystem.settings;
        //GUILayout.Label("TimesliceEvents: " + s.timesliceEvents.ToString(), Styles.BoldLabel);
        GUILayout.Label("UpdateMode: " + s.updateMode.ToString(), Styles.BoldLabel);
        GUILayout.Label("CompensateForScreenOrientation: " + s.compensateForScreenOrientation.ToString(), Styles.BoldLabel);
        GUILayout.Label("FilterNoiseOnCurrent: " + s.filterNoiseOnCurrent.ToString(), Styles.BoldLabel);
    }

    private IReadOnlyList<InputDevice> GetDevices(Type filterByType = null)
    {
        var devices = new List<InputDevice>();
        foreach (var d in InputSystem.devices)
        {
            if (filterByType != null && !filterByType.IsAssignableFrom(d.GetType()))
                continue;
            devices.Add(d);
        }
        return devices;
    }

    private void SelectDevice(InputDevice d)
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
        else if (d as Pen != null)
            m_Device = new PenDevice(d);
        else
            m_Device = new GenericDevice(d);
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
                // Automatically select a device, if there was only one of them
                var devices = GetDevices(t);
                if (devices.Count == 1)
                    SelectDevice(devices[0]);
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

        foreach (var d in GetDevices(m_SelectedType))
        {
            var name = string.Format("{0} (Type = {1}, Id = {2}, Enabled = {3})", d.displayName, d.GetType().Name, d.deviceId, d.enabled);
            if (GUILayout.Button(name, m_Device != null && m_Device.Device == d ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                SelectDevice(d);
            }
        }

        foreach (var deviceType in m_DeviceTypes)
        {
            if (deviceType == null || m_SelectedType != deviceType)
                continue;

            if (deviceType == typeof(Touchscreen))
                GUILayout.Label(string.Format("TouchScreen.current = {0}", GetDeviceInfo(Touchscreen.current)), Styles.BoldLabel);
            else if (deviceType == typeof(Keyboard))
                GUILayout.Label(string.Format("Keyboard.current = {0}", GetDeviceInfo(Keyboard.current)), Styles.BoldLabel);
            else if (deviceType == typeof(Mouse))
                GUILayout.Label(string.Format("Mouse.current = {0}", GetDeviceInfo(Mouse.current)), Styles.BoldLabel);
            else if (deviceType == typeof(Sensor))
                DoSensors();
            else if (deviceType == typeof(Gamepad))
                GUILayout.Label(string.Format("Gamepad.current = = {0}", GetDeviceInfo(Gamepad.current)), Styles.BoldLabel);
            else if (deviceType == typeof(Pen))
                GUILayout.Label(string.Format("Pen.current = {0}", GetDeviceInfo(Pen.current)), Styles.BoldLabel);
            else
                GUILayout.Label(string.Format("Unhandled device type '{0}'", deviceType.GetType().FullName), Styles.BoldLabel);
        }
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
        GUILayout.Space(40);
    }

    private void DoSensors()
    {
        GUILayout.Label(string.Format("Accelerometer.current = {0}", GetDeviceInfo(Accelerometer.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("LightSensor.current = {0}", GetDeviceInfo(LightSensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("StepCounter.current = {0}", GetDeviceInfo(StepCounter.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("Gyroscope.current = {0}", GetDeviceInfo(UnityEngine.InputSystem.Gyroscope.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("GravitySensor.current = {0}", GetDeviceInfo(GravitySensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("AttitudeSensor.current = {0}", GetDeviceInfo(AttitudeSensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("LinearAccelerationSensor.current = {0}", GetDeviceInfo(LinearAccelerationSensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("MagneticFieldSensor.current = {0}", GetDeviceInfo(MagneticFieldSensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("PressureSensor.current = {0}", GetDeviceInfo(PressureSensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("ProximitySensor.current = {0}", GetDeviceInfo(ProximitySensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("HumiditySensor.current = {0}", GetDeviceInfo(HumiditySensor.current)), Styles.BoldLabel);
        GUILayout.Label(string.Format("AmbientTemperatureSensor.current = {0}", GetDeviceInfo(AmbientTemperatureSensor.current)), Styles.BoldLabel); 
    }
}
