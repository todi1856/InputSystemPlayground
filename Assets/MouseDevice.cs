using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDevice : GenericDevice
{
    public MouseDevice(InputDevice device)
        : base(device)
    {

    }

    void ToggleCursorLockState()
    {
        if (Cursor.lockState == CursorLockMode.None) Cursor.lockState = CursorLockMode.Confined;
        else if (Cursor.lockState == CursorLockMode.Confined) Cursor.lockState = CursorLockMode.Locked;
        else if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
    }

    public override void DoUpdate()
    {
        base.DoUpdate();
        if (Keyboard.current.aKey.wasPressedThisFrame)
            ToggleCursorLockState();
    }

    protected override void DoSpecializedGUI()
    {
        if (GUILayout.Button($"Cursor LockState {Cursor.lockState}"))
            ToggleCursorLockState();
    }
}
