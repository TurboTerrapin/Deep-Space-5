/*
    Warp.cs
    - Handles warp throttle
    - Handles warp abort
    - Does nothing
    Contributor(s): Jake Schott
    Last Updated: 4/13/2025
*/


using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour, IControllable
{
    private string[] CONTROL_NAMES = { "WARP THROTTLE", "WARP ABORT" };
    private List<string> CONTROL_DESCS = new List<string>() { "DECREASE", "INCREASE", "ABORT" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5, 6 };
    private List<Button>[] BUTTON_LISTS = new List<Button>[2];

    private static HUDInfo[] hud_infos = {null, null};
    private void Start()
    {
        BUTTON_LISTS[0] = new List<Button>();
        BUTTON_LISTS[1] = new List<Button>();

        hud_infos[0] = new HUDInfo(CONTROL_NAMES[0]);
        hud_infos[1] = new HUDInfo(CONTROL_NAMES[1]);

        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false));
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], false, false));
        hud_infos[0].setButtons(BUTTON_LISTS[0]);

        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], false, false));
        hud_infos[1].setButtons(BUTTON_LISTS[1]);
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        if (current_target.name.CompareTo("warp_throttle") == 0)
        {
            return hud_infos[0];
        }
        else
        {
            return hud_infos[1];
        }
    }
  
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        //does nothing
    }
}
