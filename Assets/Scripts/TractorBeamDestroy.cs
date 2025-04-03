/*
    TractorBeamDestroy.cs
    - Meant to destroy item brought in from tractor beam
    - Does nothing
    Contributor(s): Jake Schott
    Last Updated: 4/2/2025
*/

using System.Collections.Generic;
using UnityEngine;

public class TractorBeamDestroy : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "DESTROY OBJECT";
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
