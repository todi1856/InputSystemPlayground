﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Styles
{
    static List<GUIStyle> m_AllStyles = new List<GUIStyle>();
    static GUIStyle m_BoldLabel;
    static GUIStyle m_BoldButton;
    static GUIStyle m_BoldButtonSelected;
    static float m_FontSize = 15;

    public static GUIStyle BoldLabel
    {
        get
        {
            if (m_BoldLabel == null)
            {
                m_BoldLabel = new GUIStyle(GUI.skin.label);
                m_BoldLabel.fontStyle = FontStyle.Bold;
                m_BoldLabel.fontSize = (int)m_FontSize;
                m_AllStyles.Add(m_BoldLabel);
            }
            return m_BoldLabel;
        }
    }

    public static GUIStyle BoldButton
    {
        get
        {
            if (m_BoldButton == null)
            {
                m_BoldButton = new GUIStyle(GUI.skin.button);
                m_BoldButton.fontStyle = FontStyle.Bold;
                m_BoldButton.fontSize = (int)m_FontSize;
                m_AllStyles.Add(m_BoldButton);
            }
            return m_BoldButton;
        }
    }

    public static GUIStyle BoldButtonSelecetd
    {
        get
        {
            if (m_BoldButtonSelected == null)
            {
                m_BoldButtonSelected = new GUIStyle(GUI.skin.button);
                m_BoldButtonSelected.fontStyle = FontStyle.Bold;
                m_BoldButtonSelected.fontSize = (int)m_FontSize;
                m_BoldButtonSelected.normal = m_BoldButtonSelected.onActive;
                m_BoldButtonSelected.hover = m_BoldButtonSelected.onActive;
                m_AllStyles.Add(m_BoldButtonSelected);
            }
            return m_BoldButtonSelected;
        }
    }


    public static float FontSize
    {
        set
        {
            m_FontSize = value;
            foreach(var s in m_AllStyles)
            {
                s.fontSize = (int)value;
            }
        }
        get
        {
            return (int)m_FontSize;
        }
    }
}