/*
    VerticalThrusters.cs
    - Handles inputs for vertical thrusters
    - Extends ThrusterControl.cs
    Contributor(s): Jake Schott
    Last Updated: 5/12/2025
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class VerticalThrusters : ThrusterControl, IControllable
{
    //CLASS CONSTANTS
    private float MAX_ALTITUDE = 9990f;
    private float ALTITUDE_SPEED = 50.0f;

    private string CONTROL_NAME = "VERTICAL THRUSTERS";
    private List<string> CONTROL_DESCS = new List<string>{"DESCEND", "ASCEND"};
    private List<int> CONTROL_INDEXES = new List<int>(){2, 0};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject altitude_canvas;

    private float real_altitude = 0.0f;
    private int altitude = 0;

    private List<KeyCode> keys_down = new List<KeyCode>();

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

    public float getVerticalThrusterState()
    {
        return (thruster_percentage[1] - thruster_percentage[0]);
    }

    IEnumerator adjustingThrust()
    {
        while (keys_down.Count > 0 || !checkNeutralState())
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);

            //check inputs and adjust thruster/button percentages
            for (int i = 0; i < 2; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], keys_down))
                {
                    thruster_percentage[i] = Mathf.Min(1.0f, thruster_percentage[i] + (dt * MOVE_SPEED));
                    button_push_percentage[i] = Mathf.Min(1.0f, button_push_percentage[i] + (dt * MOVE_SPEED * PUSH_SPEED));
                }
                else
                {
                    thruster_percentage[i] = Mathf.Max(0.0f, thruster_percentage[i] - (dt * MOVE_SPEED));
                    button_push_percentage[i] = Mathf.Max(0.0f, button_push_percentage[i] - (dt * MOVE_SPEED * PUSH_SPEED));
                }
            }
            //update altitude
            if (thrust_direction != 0.0f)
            {
                real_altitude += (thrust_direction * MOVE_SPEED * ALTITUDE_SPEED * dt);
                altitude = (int)real_altitude;
                if (altitude > MAX_ALTITUDE)
                {
                    altitude = (int)MAX_ALTITUDE;
                }
                else if (altitude < -MAX_ALTITUDE)
                {
                    altitude = -(int)MAX_ALTITUDE;
                }
            }

            transmitVerticalThrusterRPC(thruster_percentage[0], thruster_percentage[1], button_push_percentage[0], button_push_percentage[1], altitude);
            keys_down.Clear();
            yield return null;
        }

        thruster_coroutine = null;
    }
    private void displayAdjustment()
    {
        //adjust physical buttons
        adjustButton(thruster_buttons[0], 0);
        adjustButton(thruster_buttons[1], 1);

        //update diamond
        GameObject diamond = display_canvas.transform.GetChild(1).gameObject;
        float diamond_location = (thrust_direction + 1.0f) / 2.0f;

        diamond.transform.localPosition =
            new Vector3(Mathf.Lerp(0.055f, -0.055f, diamond_location),
                        diamond.transform.localPosition.y,
                        diamond.transform.localPosition.z);

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

    [Rpc(SendTo.Everyone)]
    private void transmitVerticalThrusterRPC(float down_thrust, float up_thrust, float down_button, float up_button, int alt)
    {
        thruster_percentage[0] = down_thrust;
        thruster_percentage[1] = up_thrust;
        button_push_percentage[0] = down_button;
        button_push_percentage[1] = up_button;
        altitude = alt;
        updateThrust();
        displayAdjustment();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (thruster_coroutine == null)
        {
            for (int i = 0; i < CONTROL_INDEXES.Count; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    thruster_coroutine = StartCoroutine(adjustingThrust());
                    return;
                }
            }
        }
    }
}