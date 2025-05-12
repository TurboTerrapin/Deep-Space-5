/*
    PowerControl.cs
    - Handles power-on/power-off procedure
    - Moves throttle lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/14/2025
*/

using System.Collections.Generic;
using UnityEngine;

public class PowerControl : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "POWER CONTROL";
    private List<string> CONTROL_DESCS = new List<string>{"ENABLE"};
    private List<int> CONTROL_INDEXES = new List<int>(){6};
    private List<Button>[] BUTTON_LISTS = new List<Button>[4];

    public List<GameObject> dials = null;
    public List<GameObject> light_indicator_groups = null;

    private List<string> ray_targets = new List<string>{"pilot_power", "tactician_power", "engineer_power", "captain_power"};
    private bool[] power_enabled = new bool[4] { false, false, false, false };
    private bool[] is_turning = new bool[4] { false, false, false, false };
    private bool[] cooling_down = new bool[4] { false, false, false, false };
    private float[] turn_timer = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        for (int i = 0; i < 4; i++)
        {
            BUTTON_LISTS[i] = new List<Button>();
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        }
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        hud_info.setButtons(BUTTON_LISTS[index]);
        return hud_info;
    }
    
    //updates knob light, adjacent circle lights (for all positions)
    private void changeIndicator(int index, bool active)
    {
        dials[index].transform.GetChild(1).GetChild(0).GetChild(1).gameObject.SetActive(active);
        for (int i = 0; i < light_indicator_groups.Count; i++)
        {
            if (i != 2) //haven't added engineer yet
            {
                light_indicator_groups[i].transform.GetChild(index).GetChild(0).GetChild(1).gameObject.SetActive(active);
            }
        }
    }
    public void Update()
    {
        float delta_time = Time.deltaTime;
        for (int i = 0; i <= 3; i++)
        {
            if (is_turning[i])
            {
                if (turn_timer[i] > 0.0f)
                {
                    turn_timer[i] -= delta_time;
                    if (power_enabled[i] == false)
                    {
                        dials[i].transform.localRotation = Quaternion.Euler(dials[i].transform.localRotation.eulerAngles.x, dials[i].transform.localRotation.eulerAngles.y, -90 + (90f - (turn_timer[i] / 1.0f * 90f)));
                    }
                    else
                    {
                        dials[i].transform.localRotation = Quaternion.Euler(dials[i].transform.localRotation.eulerAngles.x, dials[i].transform.localRotation.eulerAngles.y, 0 - (90f - (turn_timer[i] / 1.0f * 90f)));
                    }
                }
                else
                {
                    turn_timer[i] = 0.0f;
                    if (power_enabled[i] == true)
                    {
                        dials[i].transform.localRotation = Quaternion.Euler(dials[i].transform.localRotation.eulerAngles.x, dials[i].transform.localRotation.eulerAngles.y, -90f);
                    }
                    else
                    {
                        dials[i].transform.localRotation = Quaternion.Euler(dials[i].transform.localRotation.eulerAngles.x, dials[i].transform.localRotation.eulerAngles.y, 0f);
                        changeIndicator(i, true);
                    }
                    power_enabled[i] = !power_enabled[i];

                    is_turning[i] = false;
                    cooling_down[i] = true;
                    turn_timer[i] = 0.25f;
                }
            }
            else if (cooling_down[i])
            {
                if (turn_timer[i] > 0.0f)
                {
                    turn_timer[i] -= delta_time;
                }
                else
                {
                    turn_timer[i] = 0.0f;
                    cooling_down[i] = false;
                    BUTTON_LISTS[i][0].updateInteractable(true);
                    if (power_enabled[i])
                    {
                        BUTTON_LISTS[i][0].updateDesc("DISABLE");
                    }
                    else
                    {
                        BUTTON_LISTS[i][0].updateDesc("ENABLE");
                    }
                }
            }
        }
    }
    private void switch_power(int index)
    {
        turn_timer[index] = 1.0f;
        is_turning[index] = true;
        BUTTON_LISTS[index][0].toggle(0.2f);
        if (power_enabled[index] == true)
        {
            changeIndicator(index, false);
        }
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        int index = ray_targets.IndexOf(current_target.name);
        if (is_turning[index] == false && cooling_down[index] == false)
        {
            if (inputs.Contains(KeyCode.Mouse0) || inputs.Contains(KeyCode.KeypadEnter))
            {
                switch_power(index);
            }
        }
    }
}