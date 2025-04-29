/*
    VerticalThrusters.cs
    - Handles inputs for vertical thrusters
    - Extends ThrusterControl.cs
    Contributor(s): Jake Schott
    Last Updated: 4/13/2025
*/


using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VerticalThrusters : ThrusterControl, IControllable
{
    //CLASS CONSTANTS
    private float MAX_ALTITUDE = 9990f;
    private float UPDATE_DELAY = 0.02f; //time in seconds between updates

    private string CONTROL_NAME = "VERTICAL THRUSTERS";
    private List<string> CONTROL_DESCS = new List<string> { "DESCEND", "ASCEND" };
    private List<int> CONTROL_INDEXES = new List<int>() { 2, 0 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject altitude_canvas;

    private List<KeyCode> keys_down = new List<KeyCode>();
    private float delay_timer = 0.0f;

    private int altitude = 0;

    private static HUDInfo hud_info = null;

    private float verticalThrust = 0;

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

    public float getVerticalThrust()
    {
        return verticalThrust;
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

        //update altitude screen
        if (altitude >= 0)
        {
            altitude_canvas.transform.GetChild(4).gameObject.SetActive(false); //hide negative altitude mark
            altitude_canvas.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().SetText(altitude.ToString() + "m");
        }
        else
        {
            altitude_canvas.transform.GetChild(4).gameObject.SetActive(true); //show negative altitude mark
            altitude_canvas.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().SetText("-");
            altitude_canvas.transform.GetChild(4).gameObject.GetComponent<TMP_Text>().SetText(Mathf.Abs(altitude).ToString() + "m");
        }
        altitude_canvas.transform.GetChild(2).transform.localPosition = new Vector3(0.0268f, (altitude / MAX_ALTITUDE) * 0.038f, 0f);
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
                verticalThrust = 0f;
            }
            else
            {
                float temp_thrust = 0;
                buttons[1] = (keys_down.Contains(KeyCode.W) || keys_down.Contains(KeyCode.UpArrow));
                if (buttons[1]) //W to move up
                {
                    temp_thrust = 1;

                }
                buttons[0] = (keys_down.Contains(KeyCode.S) || keys_down.Contains(KeyCode.DownArrow));
                if (buttons[0]) //S to move down
                {
                    temp_thrust -= 1;

                }
                if (thrust_direction != temp_thrust)
                {
                    adjustThrust(temp_thrust);
                }
                verticalThrust = temp_thrust;

            }

            //update altitude
            if (thrust_direction != 0)
            {
                altitude += (int)thrust_direction * 10;
                if (altitude > MAX_ALTITUDE)
                {
                    altitude = (int)MAX_ALTITUDE;
                }
                else if (altitude < -MAX_ALTITUDE)
                {
                    altitude = -(int)MAX_ALTITUDE;
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