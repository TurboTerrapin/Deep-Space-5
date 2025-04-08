/*
    SecondaryInertialDampener.cs
    - Meant to enable/disable secondary inertial dampener
    - Does nothing
    Contributor(s): Jake Schott
    Last Updated: 3/30/2025
*/

using System.Collections.Generic;
using UnityEngine;

public class SecondaryInertialDampener : InertialDampener, IControllable
{
    private string CONTROL_NAME = "SECONDARY INERTIAL DAMPENER";
    private List<string> CONTROL_DESCS = new List<string>();
    private List<int> CONTROL_INDEXES = new List<int>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setInputs(CONTROL_DESCS, CONTROL_INDEXES);
    }
    public HUDInfo getHUDinfo()
    {
        return hud_info;
    }

    public void handleInputs(List<KeyCode> inputs)
    {
        //does nothing
    }
}
