
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
class InputSimulation
{
    public enum State
    {
        Idle,
        WaitingForEvent,
        WaitingForEventExpire
    }

    class EventData
    {
        public DateTime? injectStartTime;
        public DateTime? injectEndTime;

        public double GetMS()
        {
            if (injectEndTime != null && injectStartTime != null)
            {
                return ((TimeSpan)(injectEndTime - injectStartTime)).TotalMilliseconds;
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

    public InputSimulation()
    {
        m_JavaClass = new AndroidJavaObject("com.unity3d.player.InputSimulation");
        if (m_JavaClass == null)
            throw new Exception("Failed to find com.unity3d.player.InputSimulation");

        for (int i = 0; i < m_Events.Length; i++)
            m_Events[i] = new EventData();
        EnhancedTouchSupport.Enable();
        Reset();
    }

    public double AverageResponseMS
    {
        get => m_TotalEvents == 0? 0 : m_AverageResponseMS;
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
    }

    private void InjectTouchDownEvent(float x, float y)
    {
        m_JavaClass.CallStatic("injectTouchDownEvent", x, Screen.height - y);
    }

    private void InjectTouchUpEvent(float x, float y)
    {
        m_JavaClass.CallStatic("injectTouchUpEvent", x, Screen.height - y);
    }

    private void QueueEvent(int index)
    {
        m_ExpectedEventIdx = index;
        m_Events[index].injectStartTime = DateTime.Now;
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
        e.injectEndTime = DateTime.Now;

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

    private int GetInputEventIdx()
    {
        int touchCount;
        if (Configuration.Instance.OldInputEnabled)
            touchCount = Input.touchCount;
        else
            // touchCount = Touchscreen.current.touches.Count; UnityEngine.InputSystem.EnhancedTouch
            touchCount = Touch.activeTouches.Count;
        for (int i = 0; i < touchCount; i++)
        {
            int idx;
            if (Configuration.Instance.OldInputEnabled)
            {
                var touch = Input.GetTouch(i);
                idx = (int)touch.position.y;

            }
            else
            {
                // var touch = Touchscreen.current.touches[i];
               // idx = (int)touch.position.ReadValue().y;
                var touch = Touch.activeTouches[i];
                idx = (int)touch.screenPosition.y;
                
            }

            if (idx < 0 || idx > m_Events.Length - 1)
            {
                Debug.LogError($"Bad idx {idx}");
                continue;
            }
            return idx;
        }

        return -1;
    }


    public void Update()
    {
        switch(m_State)
        {
            case State.WaitingForEvent:
                {
                    var idx = GetInputEventIdx();
                    if (idx >= 0)
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
                                m_State = State.WaitingForEventExpire;
                        }
                        catch
                        {
                            m_State = State.Idle;
                            throw;
                        }
                    }
                }
                break;
            case State.WaitingForEventExpire:
                {
                    var idx = GetInputEventIdx();
                    if (idx == -1)
                    {
                        QueueEvent(m_NextEventIdx);
                    }
                }
                break;
            case State.Idle:
            default:
                break;
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
            throw new Exception("Still queing");
        Reset();

        QueueEvent(0);
    }
}