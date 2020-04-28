package com.unity3d.player;

import android.app.Instrumentation;
import android.os.SystemClock;
import android.util.DisplayMetrics;
import android.view.MotionEvent;

import java.util.ArrayList;
import java.util.List;

public class InputSimulation
{
    private static long m_LastTouchEventMS;

    private static Instrumentation m_Instrumentation;

    private static Instrumentation getInstance()
    {
        if (m_Instrumentation == null)
        {
            m_Instrumentation = new Instrumentation();
        }
        return m_Instrumentation;
    }

    public static long getTimeMS()
    {
        return SystemClock.uptimeMillis();
    }

    public static void OnTouchEventReceived()
    {
        //m_LastTouchEventMS = getTimeMS();
    }

    public static long GetLastTouchEventTime()
    {
        return m_LastTouchEventMS;
    }

    public static void injectTouchDownEvent(float x, float y)
    {
        getInstance().sendPointerSync(MotionEvent.obtain(getTimeMS(), getTimeMS(), MotionEvent.ACTION_DOWN, x , y, 0));
    }

    public static void injectTouchUpEvent(float x, float y)
    {
        getInstance().sendPointerSync(MotionEvent.obtain(getTimeMS(), getTimeMS(), MotionEvent.ACTION_UP, x , y, 0));
    }
}
