using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchscreenDevice : GenericDevice
{
    DateTime? m_InjectTime;
    int m_ReceiveType;
    AndroidJavaObject m_JavaClass;

    InputSimulation m_InputSimulation;

    public TouchscreenDevice(InputDevice device)
        : base(device)
    {
       // m_InputSimulation = new InputSimulation();
    }

    public override void DoUpdate()
    {
        base.DoUpdate();

        if (m_InputSimulation != null)
        {
            if (m_InjectTime != null && m_InjectTime < DateTime.Now)
            {
                m_InputSimulation.QueueEvents();
                m_InjectTime = null;
            }
            m_InputSimulation.Update();
        }
    }
    protected override void DoSpecializedGUI()
    {
        if (m_InputSimulation != null)
        {
            if (GUILayout.Button("Inject", Styles.BoldButton))
            {
                m_InjectTime = DateTime.Now.AddSeconds(4);
            }

            InputSimulation.EventReceiveType[] values;
            values = Configuration.Instance.OldInputEnabled ?
                new[] { InputSimulation.EventReceiveType.OldInput } :
                new[] {
                InputSimulation.EventReceiveType.NewInputViaUpdateTouchScreen,
                InputSimulation.EventReceiveType.NewInputViaUpdateEnchancedTouchScreen,
                InputSimulation.EventReceiveType.NewInputViaCallbacks,
                };



            m_ReceiveType = GUILayout.Toolbar(m_ReceiveType, values.Select(m => new GUIContent(m.ToString())).ToArray());
            m_InputSimulation.ReceiveType = values[m_ReceiveType];

            if (m_InjectTime != null && m_InjectTime > DateTime.Now)
            {
                TimeSpan timeSpan = (TimeSpan)(m_InjectTime - DateTime.Now);
                GUILayout.Label($"Inject will happen in {timeSpan.TotalSeconds}. Don't touch!");
            }
            GUILayout.Label(m_InputSimulation.SimulationState.ToString());
            GUILayout.Label($"Min: {m_InputSimulation.MinResponseMS}ms");
            GUILayout.Label($"Max: {m_InputSimulation.MaxResponseMS}ms");
            GUILayout.Label($"Average: {m_InputSimulation.AverageResponseMS}ms");
            GUILayout.Label($"Total Events: {m_InputSimulation.GetTotalEvents()}");
        }
    }
}
