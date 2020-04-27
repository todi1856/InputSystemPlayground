package com.unity3d.player;

import android.app.Instrumentation;
import android.os.SystemClock;
import android.util.DisplayMetrics;
import android.view.MotionEvent;

import java.util.ArrayList;
import java.util.List;

public class InputSimulation
{
    private static Instrumentation m_Instrumentation;

    private static Instrumentation getInstance()
    {
        if (m_Instrumentation == null)
        {
            m_Instrumentation = new Instrumentation();
        }
        return m_Instrumentation;
    }

    public static void injectTouchDownEvent(float x, float y)
    {
        getInstance().sendPointerSync(MotionEvent.obtain(SystemClock.uptimeMillis(),
                SystemClock.uptimeMillis(),MotionEvent.ACTION_DOWN, x , y, 0));
    }

    public static void injectTouchUpEvent(float x, float y)
    {
        getInstance().sendPointerSync(MotionEvent.obtain(SystemClock.uptimeMillis(),
                SystemClock.uptimeMillis(),MotionEvent.ACTION_UP, x , y, 0));
    }
}
