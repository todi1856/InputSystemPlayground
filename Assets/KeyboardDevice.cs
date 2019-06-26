using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardDevice : GenericDevice
{
    string m_Text;
    public KeyboardDevice(InputDevice device)
        : base(device)
    {
        ((Keyboard)Device).onTextInput += KeyboardDevice_onTextInput;
        m_Text = string.Empty;
    }

    public override void Dispose()
    {
        base.Dispose();
        ((Keyboard)Device).onTextInput -= KeyboardDevice_onTextInput;
    }

    private void KeyboardDevice_onTextInput(char obj)
    {
        m_Text += obj;
    }

    protected override void DoSpecializedGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Text (Length: {0})", m_Text.Length), Styles.BoldLabel);
        GUILayout.Label(m_Text);
        GUILayout.EndHorizontal();
    }
}
