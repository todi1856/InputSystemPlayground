using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Styles
{
    static GUIStyle m_BoldLabel;

    public static GUIStyle BoldLabel
    {
        get
        {
            if (m_BoldLabel == null)
            {
                m_BoldLabel = new GUIStyle(GUI.skin.label);
                m_BoldLabel.fontStyle = FontStyle.Bold;
            }
            return m_BoldLabel;
        }
    }
}
