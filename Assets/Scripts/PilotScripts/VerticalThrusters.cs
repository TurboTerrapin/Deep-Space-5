/*
    VerticalThrusters.cs
    - Handles inputs for vertical thrusters
    - Extends ThrusterControl.cs
    Contributor(s): Jake Schott
    Last Updated: 6/25/2025
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VerticalThrusters : ThrusterControl, IControllable
{
    private string CONTROL_NAME = "VERTICAL THRUSTERS";
    private List<string> CONTROL_DESCS = new List<string>{"DESCEND", "ASCEND"};
    private List<int> CONTROL_INDEXES = new List<int>(){2, 0};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject altitude_slider;
    private GameObject world_root;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;

    public HUDInfo getHUDinfo(GameObject current_target)
    {
        if (hud_info == null)
        {
            world_root = GameObject.FindGameObjectWithTag("WorldRoot");
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

            transmitVerticalThrusterRPC(thruster_percentage[0], thruster_percentage[1], button_push_percentage[0], button_push_percentage[1]);
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
        float current_altitude = world_root.transform.position.y;
        for (int i = 0; i < 21; i++)
        {
            altitude_slider.transform.GetChild(i).gameObject.SetActive(false);
        }
        int smallest_number = (((int)(current_altitude * -1.0f)) / 10) * 10;
        int next_number = smallest_number + 10;
        if (current_altitude > 0.0f)
        {
            next_number = smallest_number - 10;
        }
        altitude_slider.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().SetText(next_number.ToString() + "m");
        altitude_slider.transform.GetChild(2).transform.GetChild(0).GetComponent<TMP_Text>().SetText(smallest_number.ToString() + "m");
        List<GameObject> bars = new List<GameObject>();
        int[] marker_indices = new int[4];
        int[] corresponding_markers = new int[4];
        Debug.Log(Mathf.Abs((current_altitude - 50000.0f) % 5.0f) / 1.0f);
        int marker_index = 18 - (int)(Mathf.Abs((current_altitude - 50000.0f) % 5.0f) / 1.0f);
        if (current_altitude > 0.0f)
        {
            marker_index--;
        }
        for (int i = 0; i < 4; i++)
        {
            marker_indices[i] = marker_index - (i * 5);
        }
        if ((Mathf.Abs(current_altitude) % 10.0f < 5.0f))
        {
            corresponding_markers[0] = 0;
            corresponding_markers[1] = 1;
            corresponding_markers[2] = 2;
            corresponding_markers[3] = 3;
        }
        else
        {
            corresponding_markers[0] = 1;
            corresponding_markers[1] = 0;
            corresponding_markers[2] = 3;
            corresponding_markers[3] = 2;
        }
        if (current_altitude > 0.0f)
        {
            for (int x = 0; x < 2; x++)
            {
                int temp = corresponding_markers[3 - x];
                corresponding_markers[3 - x] = corresponding_markers[x];
                corresponding_markers[x] = temp;
            }
        }
        for (int i = 0; i < 17; i++)
        {
            bool marked = false;
            for (int x = 0; x < 4; x++)
            {
                if (i == marker_indices[x])
                {
                    bars.Add(altitude_slider.transform.GetChild(corresponding_markers[x]).gameObject);
                    marked = true;
                    break;
                }
            }
            if (marked == false)
            {
                bars.Add(altitude_slider.transform.GetChild(i + 4).gameObject);
            }
        }
        float shift = ((current_altitude % 1.0f) / 1.0f) * 0.01f; //0.01 in distance between markers equals 1 meter
        for (int i = 0; i < 17; i++)
        {
            bars[i].SetActive(true);
            bars[i].transform.localPosition = new Vector3(bars[i].transform.localPosition.x, (0.01f * i) - 0.08f + shift, 0.0f);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitVerticalThrusterRPC(float down_thrust, float up_thrust, float down_button, float up_button)
    {
        thruster_percentage[0] = down_thrust;
        thruster_percentage[1] = up_thrust;
        button_push_percentage[0] = down_button;
        button_push_percentage[1] = up_button;
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