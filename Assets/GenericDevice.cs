using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class GenericDevice : IDisposable
{
    protected enum UIType
    {
        Generic,
        Specialized,
        Events
    }

    protected struct EventData
    {
        public int id;
        public uint size;
        public string displayName;
    }

    private class ButtonControlData
    {
        internal int wasPressedThisFrameCounter;
        internal int wasReleasedThisFrameCounter;
    }

    private class ActionData
    {
        internal InputAction action;
        internal int actionsPerformed;
    }

    InputDevice m_Device;
    List<InputControl> m_Controls;
    List<Type> m_ControlTypes;
    Vector2 m_ControlScrollView;
    private InputEventTrace m_EventTrace;
    private int m_EventCount;
    private bool m_CaptureDetailedEventInfo;
    private List<EventData> m_EventQueue = new List<EventData>(100);
    Vector2 m_EventScrollPosition;
    protected UIType m_UIType;
    private Dictionary<ButtonControl, ButtonControlData> m_ButtonControls = new Dictionary<ButtonControl, ButtonControlData>();
    private Dictionary<InputControl, string> m_AdditionalInfo = new Dictionary<InputControl, string>();
    private Dictionary<InputControl, ActionData> m_Actions = new Dictionary<InputControl, ActionData>();
    private InputActionMap m_ActionMap;

    public GenericDevice(InputDevice device)
    {
        m_Device = device;
        m_Controls = m_Device.allControls.ToList();
        m_ControlTypes = m_Controls.Select(x => x.GetType()).Distinct().ToList();
        m_UIType = UIType.Specialized;
        m_CaptureDetailedEventInfo = false;
        m_ActionMap = new InputActionMap();
        m_ActionMap.Disable();
        m_ActionMap.devices = new[] {device};
        foreach (var c in m_Controls)
        {
            var action = m_ActionMap.AddAction(c.path, binding: c.path);
            action.performed += Action_performed;
            m_Actions[c] = new ActionData() { action = action };
        }
        m_ActionMap.Enable();
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        m_Actions[obj.control].actionsPerformed++;

    }

    public virtual void Dispose()
    {
        m_ActionMap.Disable();

        StopEventTracing();
    }

    public InputDevice Device
    {
        get
        {
            return m_Device;
        }
    }

    public void DoToolbar()
    {
        GUILayout.BeginHorizontal();
        foreach(var e in (UIType[])Enum.GetValues(typeof(UIType)))
        {
            if (GUILayout.Button(e.ToString(), e == m_UIType ? Styles.BoldButtonSelecetd : Styles.BoldButton))
            {
                m_UIType = e;

                // Event tracing is very costly, disable when not used
                StopEventTracing();
            }
        }
        GUILayout.EndHorizontal();
    }

    private void GenerateAdditionalInfoKeyControl(ButtonControl control)
    {
        ButtonControlData data;
        if (!m_ButtonControls.TryGetValue(control, out data))
        {
            data = new ButtonControlData();
            m_ButtonControls[control] = data;
        }

        if (control.wasPressedThisFrame)
            data.wasPressedThisFrameCounter++;
        if (control.wasReleasedThisFrame)
            data.wasReleasedThisFrameCounter++;
       

        m_AdditionalInfo[control] = string.Format("wasPressedThisFrame({0}), wasReleasedThisFrame({1}), action({2})", data.wasPressedThisFrameCounter, data.wasReleasedThisFrameCounter, m_Actions[control].actionsPerformed);
    }

    public virtual void DoUpdate()
    {
        switch (m_UIType)
        {
            case UIType.Generic:
                foreach (var c in m_Controls)
                {
                    if (c.GetType() == typeof(KeyControl) || c.GetType() == typeof(AnyKeyControl))
                        GenerateAdditionalInfoKeyControl((ButtonControl)c);

                }
            break;
        }
    }

    private string GetAdditionalInfo(InputControl control)
    {
        if (m_AdditionalInfo.ContainsKey(control))
            return m_AdditionalInfo[control];

        return "No additional info";
    }

    public virtual void DoGUI()
    {
        DoToolbar();
        if (GUILayout.Button(m_Device.enabled ? "Disable Device" : "Enable Device", Styles.BoldButton))
        {
            if (m_Device.enabled)
                InputSystem.DisableDevice(m_Device);
            else
                InputSystem.EnableDevice(m_Device);
        }

        GUILayout.Label(string.Format("{0} (Type = {1}, Id = {2}, Controls = {3})", m_Device.displayName, m_Device.GetType().Name, m_Device.deviceId, m_Controls.Count), Styles.BoldLabel);
        switch (m_UIType)
        {
            case UIType.Generic:
                m_ControlScrollView = GUILayout.BeginScrollView(m_ControlScrollView);
                foreach (var c in m_Controls)
                {
                    GUILayout.Button(string.Format("{0} (Type = {1}, Value = {2}, Info = ({3}) )", c.name, c.GetType().Name, c.ReadValueAsObject(), GetAdditionalInfo(c)), Styles.BoldButton);
                }
                GUILayout.EndScrollView();
                break;
            case UIType.Specialized:
                DoSpecializedGUI();
                break;
            case UIType.Events:
                DoEventsGUI();
                break;
        }
    }

    protected virtual void DoSpecializedGUI()
    {

    }

    private void StartEventTracing()
    {
        if (m_EventTrace != null)
            m_EventTrace.Dispose();
        m_EventCount = 0;
        m_EventQueue.Clear();
        m_EventTrace = new InputEventTrace() { deviceId = m_Device.deviceId };
        m_EventTrace.onEvent += M_EventTrace_onEvent; ;
        m_EventTrace.Enable();
    }
    
    private void StopEventTracing()
    {
        if (m_EventTrace != null)
            m_EventTrace.Dispose();
        m_EventTrace = null;
    }

    private void M_EventTrace_onEvent(InputEventPtr inputEvent)
    {
        if (m_CaptureDetailedEventInfo)
        {
            if (m_EventQueue.Count == 100)
                m_EventQueue.RemoveAt(0);

            EventData e = new EventData();
            e.id = m_EventCount;
            e.size = inputEvent.sizeInBytes;
            e.displayName = string.Format("{0}", inputEvent.type.ToString());
            m_EventQueue.Add(e);
        }
        m_EventCount++;
    }

    private void DoEventsGUI()
    {
        if (m_EventTrace == null)
            StartEventTracing();

        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Event Count: {0}", m_EventCount), Styles.BoldLabel);
        if (GUILayout.Button("Capture Detailed Info", m_CaptureDetailedEventInfo ? Styles.BoldButtonSelecetd : Styles.BoldButton))
        {
            m_CaptureDetailedEventInfo = !m_CaptureDetailedEventInfo;
        }
        if (m_CaptureDetailedEventInfo)
        {
            if (GUILayout.Button("Clear", Styles.BoldButton))
            {
                m_EventCount = 0;
                m_EventQueue.Clear();
            }
        }
        GUILayout.EndHorizontal();
        m_EventScrollPosition = GUILayout.BeginScrollView(m_EventScrollPosition);
        for (int i = m_EventQueue.Count - 1; i >= 0; i--)
        {
            var e = m_EventQueue[i];
            GUILayout.Label(string.Format("[{0}] {1} Size = {2}", e.id, e.displayName, e.size), Styles.BoldLabel);
        }
        GUILayout.EndScrollView();
    }

}
