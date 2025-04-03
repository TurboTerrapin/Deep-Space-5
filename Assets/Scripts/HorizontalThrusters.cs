/*
    HorizontalThrusters.cs
    - Handles inputs for horizontal thrusters
    - Extends ThrusterControl.cs
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class HorizontalThrusters : ThrusterControl, IControllable
{
    private string CONTROL_NAME = "HORIZONTAL THRUSTERS";
    private List<string> CONTROL_DESCS = new List<string> {"MOVE LEFT", "MOVE RIGHT"};
    private List<int> CONTROL_INDEXES = new List<int>() {1, 3};

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;

    public HUDInfo getHUDinfo()
    {
        if (hud_info == null)
        {
            hud_info = new HUDInfo(CONTROL_NAME);
            hud_info.setInputs(CONTROL_DESCS, CONTROL_INDEXES);
        }
        return hud_info;
    }
    void FixedUpdate()
    {
        if (keys_down.Count == 0)
        {
            buttons[0] = false;
            buttons[1] = false;
        }
        else
        {
            float temp_thrust = 0;
            buttons[1] = (keys_down.Contains(KeyCode.D) || keys_down.Contains(KeyCode.RightArrow));
            if (buttons[1]) //D to move right
            {
                temp_thrust = 1;

            }
            buttons[0] = (keys_down.Contains(KeyCode.A) || keys_down.Contains(KeyCode.LeftArrow));
            if (buttons[0]) //A to move left
            {
                temp_thrust -= 1;
            }
            if (thrust_direction != temp_thrust)
            {
                adjustThrust(temp_thrust);
            }
        }
        adjustButton(physical_buttons[0], 0);
        adjustButton(physical_buttons[1], 1);
        GameObject diamond = display_canvas.transform.GetChild(1).gameObject;
        if (thrust_direction == 0) //bring back to center
        {
            if (diamond.transform.localPosition.x > 0f)
            {
                diamond.transform.localPosition = new Vector3(Mathf.Max(0, diamond.transform.localPosition.x - 0.0055f), diamond.transform.localPosition.y, diamond.transform.localPosition.z);
            }
            else if (diamond.transform.localPosition.x < 0f)
            {
                diamond.transform.localPosition = new Vector3(Mathf.Min(0, diamond.transform.localPosition.x + 0.0055f), diamond.transform.localPosition.y, diamond.transform.localPosition.z);
            }
        }
        else //move to one side
        {
            if (thrust_direction == -1)
            {
                diamond.transform.localPosition = new Vector3(Mathf.Min(0.055f, diamond.transform.localPosition.x + 0.0055f), diamond.transform.localPosition.y, diamond.transform.localPosition.z);
            }
            else
            {
                diamond.transform.localPosition = new Vector3(Mathf.Max(-0.055f, diamond.transform.localPosition.x - 0.0055f), diamond.transform.localPosition.y, diamond.transform.localPosition.z);
            }
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        keys_down = inputs;
    }
}
