/*
    DirectionalShifter.cs
    - Handles shifting between forward and reverse
    - Moves shift lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 5/7/2025
*/

using System.Collections.Generic;
using UnityEngine;
public class DirectionalShifter : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "DIRECTIONAL SHIFTER";
    private List<string> CONTROL_DESCS = new List<string> { "SHIFT" };
    private List<int> CONTROL_INDEXES = new List<int>() { 6 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject lever;
    public GameObject directional_arrow; //on the speedometer screen
    public GameObject forward_indicator;
    public GameObject reverse_indicator;

    private List<KeyCode> keys_down = new List<KeyCode>();
    private float cooldown_timer = 1.0f;

    private bool in_reverse = false; //true means in reverse, false means forward
    private bool cooling_down = false;
    private bool shifting = false;
    private float shift_percentage = 1.0f; //1 is forward, 0 is reverse
    private Vector3 forward_pos;
    private Vector3 reverse_pos = new Vector3(0f, -0.0133f, -17.889f);

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        hud_info.setButtons(BUTTONS);

        forward_pos = lever.transform.position;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }
    private void displayAdjustment()
    {
        if (shift_percentage > 0.4f && shift_percentage < 0.6f) //vertical change
        {
            float percent_to_top = (shift_percentage - 0.4f) / 0.2f;
            lever.transform.position = new Vector3(reverse_pos.x + (forward_pos.x - reverse_pos.x) * percent_to_top, reverse_pos.y + (forward_pos.y - reverse_pos.y) * percent_to_top, lever.transform.position.z);
        }
        else
        {
            float percent_to_center = (shift_percentage / 0.4f) * 0.5f;
            if (shift_percentage > 0.6f)
            {
                percent_to_center = (((shift_percentage - 0.6f) / 0.4f) * 0.5f) + 0.5f;
            }
            lever.transform.position = new Vector3(lever.transform.position.x, lever.transform.position.y, reverse_pos.z + (forward_pos.z - reverse_pos.z) * percent_to_center);
        }
        forward_indicator.SetActive(!in_reverse);
        reverse_indicator.SetActive(in_reverse);
        if (in_reverse)
        {
            directional_arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            directional_arrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
    public void Update()
    {
        //check inputs
        if (keys_down.Contains(KeyCode.Mouse0) || keys_down.Contains(KeyCode.KeypadEnter))
        {
            shifting = true;
        }
        else
        {
            shifting = false;
        }

        //adjust shift percentage
        if (cooling_down == false) //not cooling down
        {
            float temp_shift_percentage = shift_percentage;
            if (shifting == true)
            {
                if (in_reverse == false) //moving towards reverse
                {
                    shift_percentage = Mathf.Max(0.0f, shift_percentage - Time.deltaTime * 0.6f);
                }
                else //moving towards forward
                {
                    shift_percentage = Mathf.Min(1.0f, shift_percentage + Time.deltaTime * 0.6f);
                }
 
            }
            else
            {
                if (shift_percentage != 0.0f && shift_percentage != 1.0f)
                {
                    if (shift_percentage > 0.5f)
                    {
                        shift_percentage = Mathf.Min(1.0f, shift_percentage + Time.deltaTime * 0.6f);
                    }
                    else
                    {
                        shift_percentage = Mathf.Max(0.0f, shift_percentage - Time.deltaTime * 0.6f);
                    }
                }
            }
            if (temp_shift_percentage != shift_percentage) //shift percentage has changed
            {
                if (shift_percentage == 1.0f || shift_percentage == 0.0f)
                {
                    //potentially shifted
                    if ((shift_percentage == 1.0f && in_reverse == true) || (shift_percentage == 0.0f && in_reverse == false))
                    {
                        BUTTONS[0].updateInteractable(false);
                        in_reverse = !in_reverse;
                        cooling_down = true;
                    }
                }
                displayAdjustment();
            }
        }
        else
        {
            if (cooldown_timer <= 0.0f)
            {
                cooldown_timer = 1.0f;
                cooling_down = false;
                BUTTONS[0].updateInteractable(true);
            }
            else
            {
                cooldown_timer -= Time.deltaTime;
            }
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
    }
}
