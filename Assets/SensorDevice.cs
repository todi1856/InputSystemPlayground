using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SensorDevice : GenericDevice
{
    public SensorDevice(InputDevice device)
        : base(device)
    {

    }

    protected override void DoSpecializedGUI()
    {
        DoLightSensor();
    }

    private void DoLightSensor()
    {
        var lightSensor = Device as LightSensor;

        if (lightSensor == null)
            return;

        GUILayout.Label("LightLevel: " + lightSensor.lightLevel.ReadValueAsObject(), Styles.BoldLabel);
    }

}
