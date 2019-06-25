using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GenericDevice : IDisposable
{
    InputDevice m_Device;
    List<InputControl> m_Controls;
    List<Type> m_ControlTypes;
    Vector2 m_ControlScrollView;

    public GenericDevice(InputDevice device)
    {
        m_Device = device;
        m_Controls = m_Device.allControls.ToList();
        m_ControlTypes = m_Controls.Select(x => x.GetType()).Distinct().ToList(); 
    }

    public virtual void Dispose()
    {

    }

    public void DoGUI()
    {
        GUILayout.Label(string.Format("{0} (Type = {1}, Id = {2}, Controls = {3})", m_Device.displayName, m_Device.GetType().Name, m_Device.id, m_Controls.Count), Styles.BoldLabel);

        m_ControlScrollView = GUILayout.BeginScrollView(m_ControlScrollView);
        foreach(var c in m_Controls)
        {
            GUILayout.Button(string.Format("{0} (Type = {1}, Value = {2})", c.name, c.GetType().Name, c.ReadValueAsObject()));
        }
        GUILayout.EndScrollView();
    }

}
