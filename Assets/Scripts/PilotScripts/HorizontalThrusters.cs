/*
    HorizontalThrusters.cs
    - Handles inputs for horizontal thrusters
    - Extends ThrusterControl.cs
    Contributor(s): Jake Schott
    Last Updated: 4/13/2025
*/


using UnityEngine;
using System.Collections.Generic;

public class HorizontalThrusters : ThrusterControl, IControllable
{
    //CLASS CONSTANTS
    private float UPDATE_DELAY = 0.02f; //time in seconds between updates

    private string CONTROL_NAME = "HORIZONTAL THRUSTERS";
    private List<string> CONTROL_DESCS = new List<string> { "MOVE LEFT", "MOVE RIGHT" };
    private List<int> CONTROL_INDEXES = new List<int>() { 1, 3 };
    private List<Button> BUTTONS = new List<Button>();

    private List<KeyCode> keys_down = new List<KeyCode>();
    private float delay_timer = 0.0f;

    private static HUDInfo hud_info = null;

    public HUDInfo getHUDinfo(GameObject current_target)
    {
        if (hud_info == null)
        {
            hud_info = new HUDInfo(CONTROL_NAME);
            BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
            BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
            hud_info.setButtons(BUTTONS);
        }
        return hud_info;
    }
    private void displayAdjustment()
    {
        //adjust physical buttons
        adjustButton(physical_buttons[0], 0);
        adjustButton(physical_buttons[1], 1);

        //update corresponding thruster screen
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
    }
    void Update()
    {
        delay_timer -= Time.deltaTime;
        if (delay_timer <= 0.0f)
        {
            //ensure thrust is updated
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

            //display changes
            displayAdjustment();

            //reset timer
            delay_timer = UPDATE_DELAY;
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        keys_down = inputs;
    }
}