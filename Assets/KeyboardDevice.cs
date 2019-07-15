using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using GUILayout = UnityEngine.GUILayout;

public class KeyboardDevice : GenericDevice
{
    private class KeyData
    {
        internal int keyDown;
        internal int keyUp;
        internal int keyCounter;
    }

    string m_Text;
    private string m_GUIText;
    private Dictionary<KeyCode, KeyData> m_Keys = new Dictionary<KeyCode, KeyData>();
    private Vector2 m_KeysScrollPosition;

    public KeyboardDevice(InputDevice device)
        : base(device)
    {
        ((Keyboard)Device).onTextInput += KeyboardDevice_onTextInput;
        m_Text = string.Empty;
        m_GUIText = string.Empty;
    }

    public override void Dispose()
    {
        base.Dispose();
        ((Keyboard)Device).onTextInput -= KeyboardDevice_onTextInput;
    }

    public override void DoUpdate()
    {
        base.DoUpdate();
        if (Configuration.Instance.OldInputEnabled == false)
            return;
        foreach (var key in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
        {
            KeyData k;
            if (m_Keys.TryGetValue(key, out k) == false)
            {
                k = new KeyData();
                m_Keys[key] = k;
            }

            if (Input.GetKeyDown(key))
                k.keyDown++;
            if (Input.GetKeyUp(key))
                k.keyUp++;
            if (Input.GetKey(key))
                k.keyCounter++;
        }
    }

    private void KeyboardDevice_onTextInput(char obj)
    {
        m_Text += obj;
    }

    protected override void DoSpecializedGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Text (Length: {0})", m_Text.Length), Styles.BoldLabel);
        GUILayout.Label(m_Text, Styles.BoldLabel);
        GUILayout.EndHorizontal();
        m_GUIText = GUILayout.TextField(m_GUIText, Styles.BoldTextField);

        if (Configuration.Instance.OldInputEnabled == false)
        {
            GUILayout.Label("Old Input Disabled", Styles.BoldLabel);
            return;
        }
        m_KeysScrollPosition = GUILayout.BeginScrollView(m_KeysScrollPosition);
        foreach (var key in m_Keys)
        {
            var k = key.Value;
            if (k.keyCounter == 0 || k.keyDown == 0 || k.keyUp == 0)
                continue;
            GUILayout.Label(string.Format("{0} down - {1}, up - {2}, counter - {3}", key.Key, k.keyDown, k.keyUp, k.keyCounter), Styles.BoldLabel);
        }

        GUILayout.EndScrollView();

    }
}
