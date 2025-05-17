/*
    InputOutputToggle.cs
    - Meant to switch viewer from write/read mode
    - Does nothing
    Contributor(s): Jake Schott
    Last Updated: 5/17/2025
*/

using System.Collections.Generic;
using UnityEngine;

public class InputOutputToggle : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "INPUT/OUTPUT TOGGLE";
    private List<string> CONTROL_DESCS = new List<string>() {"FLIP"};
    private List<int> CONTROL_INDEXES = new List<int>() {6};
    private List<Button> BUTTONS = new List<Button>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false));
        hud_info.setButtons(BUTTONS);
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        //does nothing
    }
}