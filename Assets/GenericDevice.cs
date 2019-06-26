using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public GenericDevice(InputDevice device)
    {
        m_Device = device;
        m_Controls = m_Device.allControls.ToList();
        m_ControlTypes = m_Controls.Select(x => x.GetType()).Distinct().ToList();
        m_UIType = UIType.Specialized;
        m_CaptureDetailedEventInfo = false;
    }

    public virtual void Dispose()
    {
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

        GUILayout.Label(string.Format("{0} (Type = {1}, Id = {2}, Controls = {3})", m_Device.displayName, m_Device.GetType().Name, m_Device.id, m_Controls.Count), Styles.BoldLabel);
        switch (m_UIType)
        {
            case UIType.Generic:
                m_ControlScrollView = GUILayout.BeginScrollView(m_ControlScrollView);
                foreach (var c in m_Controls)
                {
                    GUILayout.Button(string.Format("{0} (Type = {1}, Value = {2})", c.name, c.GetType().Name, c.ReadValueAsObject()), Styles.BoldButton);
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
        m_EventTrace = new InputEventTrace() { deviceId = m_Device.id };
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
