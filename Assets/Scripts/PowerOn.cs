/*
    PowerOn.cs
    - Handles power-on/power-off procedure
    - Moves throttle lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/2/2025
*/

using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowerOn : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "POWER CONTROL";
    private List<string> CONTROL_DESCS = new List<string>{"ENABLE"};
    private List<int> CONTROL_INDEXES = new List<int>(){6};

    public GameObject knob;
    public GameObject knob_indicator_canvas; //used to update knob
    public GameObject pilot_indicator_canvas; //used to update thing next to knob

    private bool power_enabled = false;
    private bool is_turning = false;
    private float turn_timer = 0.0f;

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
    public void FixedUpdate()
    {
        if (is_turning)
        {
            if (turn_timer > 0.0f)
            {
                turn_timer -= Time.deltaTime;
                if (power_enabled == false)
                {
                    knob.transform.localRotation = Quaternion.Euler(-68.739f, -90f, -90 + (90f - (turn_timer / 1.0f * 90f)));
                }
                else
                {
                    knob.transform.localRotation = Quaternion.Euler(-68.739f, -90f, 0 - (90f - (turn_timer / 1.0f * 90f)));
                }
            }
            else
            { 
                turn_timer = 0.0f;
                if (power_enabled == true)
                {
                    knob.transform.localRotation = Quaternion.Euler(-68.739f, -90f, -90f);
                }
                else
                {
                    knob.transform.localRotation = Quaternion.Euler(-68.739f, -90f, 0f);
                    pilot_indicator_canvas.transform.GetChild(1).gameObject.SetActive(true);
                    knob_indicator_canvas.transform.GetChild(1).gameObject.SetActive(true);
                }
                power_enabled = !power_enabled;
                CONTROL_INDEXES = new List<int> { 6 };
                if (power_enabled)
                {
                    CONTROL_DESCS = new List<string> { "DISABLE" };
                }
                else
                {
                    CONTROL_DESCS = new List<string> { "ENABLE" };
                }
                hud_info = new HUDInfo(CONTROL_NAME);
                hud_info.setInputs(CONTROL_DESCS, CONTROL_INDEXES);
                is_turning = false;
            }
        }
    }
    private void switch_power()
    {
        CONTROL_DESCS.Clear();
        CONTROL_INDEXES.Clear();
        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setInputs(CONTROL_DESCS, CONTROL_INDEXES);
        turn_timer = 1.0f;
        is_turning = true;
        if (power_enabled == true)
        {
            pilot_indicator_canvas.transform.GetChild(1).gameObject.SetActive(false);
            knob_indicator_canvas.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        if (is_turning == false)
        {
            if (inputs.Contains(KeyCode.Mouse0) || inputs.Contains(KeyCode.KeypadEnter)) //To turn the knob
            {
                switch_power();
            }
        }
    }
}
