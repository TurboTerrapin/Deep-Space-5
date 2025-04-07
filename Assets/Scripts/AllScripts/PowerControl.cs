/*
    PowerControl.cs
    - Handles power-on/power-off procedure
    - Moves throttle lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/6/2025
*/

using System.Collections.Generic;
using UnityEngine;

public class PowerControl : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "POWER CONTROL";
    private List<string> CONTROL_DESCS = new List<string>{"ENABLE"};
    private List<int> CONTROL_INDEXES = new List<int>(){6};

    public GameObject knob;
    public GameObject knob_indicator_canvas; //used to update knob
    public GameObject pilot_indicator_canvas; //used to update thing next to knob

    private bool power_enabled = false;
    private bool is_turning = false;
    private bool cooling_down = false;
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
    public void Update()
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

                is_turning = false;
                cooling_down = true;
                turn_timer = 0.25f;
            }
        }
        else if (cooling_down) { }
        {
            if (turn_timer > 0.0f)
            {
                turn_timer -= Time.deltaTime;
            }
            else
            {
                turn_timer = 0.0f;
                cooling_down = false;
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
            }
        }
    }
    private void switch_power()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setInputs(new List<string>(), new List<int>());
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
        if (is_turning == false && cooling_down == false)
        {
            if (inputs.Contains(KeyCode.Mouse0) || inputs.Contains(KeyCode.KeypadEnter)) //To turn the knob
            {
                switch_power();
            }
        }
    }
}
