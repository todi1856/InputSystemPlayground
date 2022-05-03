using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadDevice : GenericDevice
{
    public GamepadDevice(InputDevice device)
        : base(device)
    {

    }

    protected override void DoSpecializedGUI()
    {
        var g = ((Gamepad)Device);
        GUILayout.Label($"DPad L: {g.dpad.left.ReadValue()}, R: {g.dpad.right.ReadValue()}, D: {g.dpad.down.ReadValue()}, U: {g.dpad.up.ReadValue()}");
        GUILayout.Label($"Stick L: {g.leftStick.ReadValue()} ({g.leftStickButton.ReadValue()}) R: {g.rightStick.ReadValue()} ({g.rightStickButton.ReadValue()})");
        GUILayout.Label($"Sq: {g.buttonEast.ReadValue()}, Cr: {g.buttonWest.ReadValue()}, Tr: {g.buttonNorth.ReadValue()}, X: {g.buttonSouth.ReadValue()}");
        GUILayout.Label($"Trigger L: {g.leftTrigger.ReadValue()}, R: {g.rightTrigger.ReadValue()}");
        GUILayout.Label($"Shoulder L: {g.leftShoulder.ReadValue()}, R: {g.rightShoulder.ReadValue()}");
        GUILayout.Label($"Start: {g.startButton.ReadValue()}, Select: {g.selectButton.ReadValue()}");
    }
}
