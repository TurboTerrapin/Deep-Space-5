/*
    VerticalThrusters.cs
    - Handles inputs for vertical thrusters
    - Extends ThrusterControl.cs
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class VerticalThrusters : ThrusterControl, IControllable
{
    private float MAX_ALTITUDE = 9990f;

    private string CONTROL_NAME = "VERTICAL THRUSTERS";
    private List<string> CONTROL_DESCS = new List<string>{"DESCEND", "ASCEND"};
    private List<int> CONTROL_INDEXES = new List<int>(){2, 0};

    public GameObject altitude_canvas;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private int altitude = 0;

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
            if (altitude >= 0)
            {
                altitude_canvas.transform.GetChild(4).gameObject.SetActive(false); //hide negative altitude
                altitude_canvas.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().SetText(altitude.ToString() + "m");
            }
            else
            {
                altitude_canvas.transform.GetChild(4).gameObject.SetActive(true); //show negative altitude
                altitude_canvas.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().SetText("-");
                altitude_canvas.transform.GetChild(4).gameObject.GetComponent<TMP_Text>().SetText(Mathf.Abs(altitude).ToString() + "m");
            }
            altitude_canvas.transform.GetChild(2).transform.localPosition = new Vector3(0f, (altitude / MAX_ALTITUDE) * 0.038f, 0f);
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        keys_down = inputs;
    }
}
