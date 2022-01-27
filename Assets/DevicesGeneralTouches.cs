using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public partial class Devices
{
    private static readonly TouchPhase[] Phases = (TouchPhase[])Enum.GetValues(typeof(TouchPhase));
    public class TouchPhaseStats
    {
        Dictionary<TouchPhase, int> m_PhaseCount;
        public Dictionary<TouchPhase, int> Phase => m_PhaseCount;

        public TouchPhaseStats()
        {
            m_PhaseCount = new Dictionary<TouchPhase, int>();
            foreach (var p in Phases)
            {
                m_PhaseCount[p] = 0;
            }
        }
    }

    TouchPhaseStats m_PhaseStats = new TouchPhaseStats();
    HashSet<int> m_UniqueTouchIds = new HashSet<int>();

    private void DoGenericTouchScreenUpdate()
    {
        for (int i = 0; i < Touch.activeTouches.Count; i++)
        {
            var t = Touch.activeTouches[i];
            m_PhaseStats.Phase[t.phase]++;

            m_UniqueTouchIds.Add(t.touchId);
        }
    }

    private void DoGenericTouchScreenGUI()
    {
        GUILayout.Label(string.Format("TouchScreen.current = (Id = {0})", Touchscreen.current != null ? Touchscreen.current.deviceId.ToString() : "<null>"), Styles.BoldLabel);
        GUILayout.Label($"Enhanced Touch (Active Fingers {Touch.activeFingers.Count}, Active Touches {Touch.activeTouches.Count}, Unique Ids {m_UniqueTouchIds.Count}");

        foreach (var p in Phases)
        {
            if (m_PhaseStats.Phase[p] == 0)
                continue;
            GUILayout.Label($"{p} - {m_PhaseStats.Phase[p]}");
        }

        foreach (var t in Touch.activeTouches)
        {
            GUILayout.Label($"{t.touchId} - {t.screenPosition}");
        }
    }

}