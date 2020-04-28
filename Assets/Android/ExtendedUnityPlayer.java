package com.unity3d.player;

import android.content.Context;
import android.view.MotionEvent;

public class ExtendedUnityPlayer extends UnityPlayer
{
    public ExtendedUnityPlayer(Context context, IUnityPlayerLifecycleEvents lifecycleEventListener)
    {
        super(context, lifecycleEventListener);
    }

    @Override public boolean onTouchEvent(MotionEvent event)
    {
        if (event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_POINTER_DOWN)
            InputSimulation.OnTouchEventReceived();
        return super.onTouchEvent(event);
    }
}