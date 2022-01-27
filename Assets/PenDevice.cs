using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PenDevice : GenericDevice
{
    public PenDevice(InputDevice device)
        : base(device)
    {

    }
    protected override void DoSpecializedGUI()
    {
        var p = ((Pen)Device);
        var t = p.tilt.ReadValue();
        GUILayout.Label($"Position = {p.position.ReadValue()}, Tip = {p.tip.ReadValue()}");
        GUILayout.Label($"Tilt = {t}, Twist = {p.twist.ReadValue()}");
        GUILayout.Label($"Pressure = {p.pressure.ReadValue()}");
        GUILayout.Label($"Eraser = {p.eraser.ReadValue()}");
        GUILayout.Label($"Barrel1 = {p.firstBarrelButton.ReadValue()}");
    }
}
