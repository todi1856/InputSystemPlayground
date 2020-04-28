
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
class InputSimulation
{
    public enum EventReceiveType
    {
        OldInput,
        NewInputViaUpdateTouchScreen,
        NewInputViaUpdateEnchancedTouchScreen,
        NewInputViaCallbacks
    }

    public enum State
    {
        Idle,
        WaitingForEvent,
        WaitingForEventExpire
    }

    class EventData
    {
        public long? injectStartTime;
        public long? injectEndTime;

        public long GetMS()
        {
            if (injectEndTime != null && injectStartTime != null)
            {
                return (long)(injectEndTime - injectStartTime);
            }

            return 0;
        }

        public void Reset()
        {
            injectEndTime = null;
            injectStartTime = null;
        }

        public bool IsValid()
        {
            return injectStartTime != null && injectEndTime != null;
        }

        public override string ToString()
        {
            return $"{GetMS()} ms";
        }
    }

    public const int kMaxInputEvents = 100;

    AndroidJavaObject m_JavaClass;
    EventData[] m_Events = new EventData[kMaxInputEvents];
    State m_State;
    EventReceiveType m_EventReceiveType;
    int m_ExpectedEventIdx;
    int m_NextEventIdx;
    double m_MinResponseMS;
    double m_MaxResponseMS;
    double m_AverageResponseMS;
    int m_TotalEvents;

    public State SimulationState
    {
        get
        {
            return m_State;
        }
    }

    public EventReceiveType ReceiveType
    {
        set
        {
            if (m_EventReceiveType == value)
                return;
            m_EventReceiveType = value;
            Reset();
        }
        get
        {
            return m_EventReceiveType;
        }
    }

    public InputSimulation()
    {
        m_JavaClass = new AndroidJavaObject("com.unity3d.player.InputSimulation");
        if (m_JavaClass == null)
            throw new Exception("Failed to find com.unity3d.player.InputSimulation");

        for (int i = 0; i < m_Events.Length; i++)
            m_Events[i] = new EventData();

        if (Configuration.Instance.OldInputEnabled)
            m_EventReceiveType = EventReceiveType.OldInput;
        else
            m_EventReceiveType = EventReceiveType.NewInputViaCallbacks;

        Reset();
    }

    private unsafe void InputSystem_onEvent(UnityEngine.InputSystem.LowLevel.InputEventPtr eventPtr, InputDevice device)
    {
        if (m_State == State.Idle || m_EventReceiveType != EventReceiveType.NewInputViaCallbacks)
            return;

        var touchScreenDevice = device as Touchscreen;
        if (touchScreenDevice == null)
            return;

        if (!eventPtr.IsA<StateEvent>())
            return;
        var stateEvent = StateEvent.From(eventPtr);
        if (stateEvent->stateFormat != TouchState.Format)
            return;
        var touchState = (TouchState*)stateEvent->state;

        if (touchState->phase == UnityEngine.InputSystem.TouchPhase.Began)
            HandleEventReceive((int)touchState->position.y);
    }

    public double AverageResponseMS
    {
        get => m_TotalEvents == 0 ? 0 : m_AverageResponseMS;
    }

    public double MinResponseMS
    {
        get => m_TotalEvents == 0 ? 0 : m_MinResponseMS;
    }

    public double MaxResponseMS
    {
        get => m_TotalEvents == 0 ? 0 : m_MaxResponseMS;
    }


    void Reset()
    {
        foreach(var e in m_Events)
        {
            e.Reset();
        }
        m_State = State.Idle;

        m_AverageResponseMS = 0;
        m_MinResponseMS = double.MaxValue;
        m_MaxResponseMS = double.MinValue;
        m_TotalEvents = 0;

        EnhancedTouchSupport.Disable();
        InputSystem.onEvent -= InputSystem_onEvent;

        switch (m_EventReceiveType)
        {
            case EventReceiveType.NewInputViaUpdateEnchancedTouchScreen:
                EnhancedTouchSupport.Enable();
                break;
            case EventReceiveType.NewInputViaCallbacks:
                InputSystem.onEvent += InputSystem_onEvent;
                break;
        }

    }

    private void InjectTouchDownEvent(float x, float y)
    {
        m_JavaClass.CallStatic("injectTouchDownEvent", x, Screen.height - y);
    }

    private void InjectTouchUpEvent(float x, float y)
    {
        m_JavaClass.CallStatic("injectTouchUpEvent", x, Screen.height - y);
    }

    private long GetLastReceivedEventTime()
    {
        return m_JavaClass.CallStatic<long>("GetLastTouchEventTime");
    }

    private long GetTimeMS()
    {
        return m_JavaClass.CallStatic<long>("getTimeMS");
    }

    private void QueueEvent(int index)
    {
        m_ExpectedEventIdx = index;
        m_Events[index].injectStartTime = GetTimeMS();
        // Add 0.1f due rounding problems
        InjectTouchDownEvent(2, index + 0.1f);
        InjectTouchUpEvent(2, index + 0.1f);

        m_State = State.WaitingForEvent;
    }

    private void ReceivedEvent(int index)
    {
        if (m_ExpectedEventIdx != index)
        {
            throw new Exception($"Expected {m_ExpectedEventIdx}, received {index}");
        }

        var e = m_Events[index];
        if (e.injectEndTime != null)
        {
            throw new Exception($"Input event {index} already received an event");
        }


        e.injectStartTime = GetLastReceivedEventTime();
        e.injectEndTime = GetTimeMS();

        m_TotalEvents = 0;
        double totalMs = 0;
        for (int i = 0; i < index + 1; i++)
        {
            if (!m_Events[i].IsValid())
                continue;

            var time = m_Events[i].GetMS();
            m_MinResponseMS = Math.Min(m_MinResponseMS, time);
            m_MaxResponseMS = Math.Max(m_MaxResponseMS, time);
            totalMs += time;
            m_TotalEvents++;
        }
            
        m_AverageResponseMS = m_TotalEvents > 0 ? totalMs / m_TotalEvents : 0;

    }

    private void ValidateEvents()
    {
        var startIdx = m_Events.Length;
        for (int i = 0; i < m_Events.Length; i++)
        {
            if (m_Events[i].injectEndTime == null)
            {
                startIdx = i;
                break;
            }
        }

        for (int i = startIdx; i < m_Events.Length; i++)
        {
            if (m_Events[i].injectEndTime != null)
                throw new Exception($"Skipped events detected, expected events [0, {startIdx}] to be the only valid, but found valid event at {i}");
        }
    }

    private bool WaitForTouchExpire()
    {
        return m_EventReceiveType == EventReceiveType.OldInput || m_EventReceiveType == EventReceiveType.NewInputViaUpdateEnchancedTouchScreen;
    }

    private int GetInputEventIdxViaOldInput()
    {
        if (!Configuration.Instance.OldInputEnabled)
            return -1;
        if (Input.touchCount == 0)
            return -1;

        var touch = Input.GetTouch(0);
        return (int)touch.position.y;
 
    }

    private int GetInputEventIdxViaNewInputTouchScreen()
    {
        if (!Configuration.Instance.NewInputEnabled)
            return -1;
        if (Touchscreen.current.touches.Count == 0)
            return -1;
        var touch = Touchscreen.current.touches[0];
        var idx = (int)touch.position.ReadValue().y;
        // TouchScreen doesn't clear data, actually it can hold garbage data, not sure why, that's why we have this check below
        if (idx != m_ExpectedEventIdx)
            return -1;
        return idx;
    }

    private int GetInputEventIdxViaNewInputEnchancedTouch()
    {
        if (!Configuration.Instance.NewInputEnabled)
            return -1;
        if (Touch.activeTouches.Count == 0)
            return -1;

        var touch = Touch.activeTouches[0];
        return (int)touch.screenPosition.y;
    }


    private int GetInputEventIdx()
    {
        int idx = -1;
        switch (m_EventReceiveType)
        {
            case EventReceiveType.OldInput:
                idx = GetInputEventIdxViaOldInput();
                break;
            case EventReceiveType.NewInputViaUpdateTouchScreen:
                idx = GetInputEventIdxViaNewInputTouchScreen();
                break;
            case EventReceiveType.NewInputViaUpdateEnchancedTouchScreen:
                idx = GetInputEventIdxViaNewInputEnchancedTouch();
                break;
        }
        
        // Note: -1 is valid
        if (idx < -1 || idx > m_Events.Length - 1)
        {
            Debug.LogError($"Bad idx {idx}");
        }
        return idx;
    }

    private void HandleEventReceive(int idx)
    {
        try
        {
            ReceivedEvent(idx);
            ValidateEvents();
            m_NextEventIdx = idx + 1;
            // We're done
            if (m_NextEventIdx == m_Events.Length)
                m_State = State.Idle;
            else
            {
                if (WaitForTouchExpire())
                    m_State = State.WaitingForEventExpire;
                else
                    QueueEvent(m_NextEventIdx);
            }
        }
        catch
        {
            m_State = State.Idle;
            throw;
        }
    }

    public void Update()
    {
        if (m_EventReceiveType == EventReceiveType.NewInputViaCallbacks)
            return;

        if (m_State == State.WaitingForEvent)
        {
            var idx = GetInputEventIdx();
            if (idx == m_ExpectedEventIdx)
            {
                HandleEventReceive(idx);
            }
        }

        // Don't else if here, since if there was a state switch, we want to handle it immediately
        if (m_State == State.WaitingForEventExpire)
        {
            var idx = GetInputEventIdx();
            if (idx == -1)
            {
                QueueEvent(m_NextEventIdx);
            }
        }
    }

    public int GetTotalEvents()
    {
        int total = 0;
        for (int i = 0; i < m_Events.Length; i++)
        {
            if (!m_Events[i].IsValid())
                continue;
            total++;
        }
        return total;
    }

    public void QueueEvents()
    {
        if (m_State != State.Idle)
        {
            // Give time for previous events to come
            Thread.Sleep(500);
        }
        Reset();
        QueueEvent(0);
    }
}