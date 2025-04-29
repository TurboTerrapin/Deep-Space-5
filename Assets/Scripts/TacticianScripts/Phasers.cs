/*
    Phasers.cs
    - Handles short-and-long-range phasers
    Contributor(s): Jake Schott
    Last Updated: 4/26/2025
*/

using UnityEngine;
using System.Collections.Generic;

public class Phasers : MonoBehaviour, IControllable
{
    private string[] CONTROL_NAMES = new string[]{"SHORT-RANGE PHASERS", "LONG-RANGE PHASERS", "LONG-RANGE PHASER DIRECTION", "PHASER POWER SWITCHES"};
    private List<string> CONTROL_DESCS = new List<string>{"REDUCE", "ENERGIZE", "ROTATE LEFT", "ROTATE RIGHT", "LONG-RANGE", "SHORT-RANGE LEFT", "SHORT-RANGE RIGHT"};
    private List<int> CONTROL_INDEXES = new List<int>(){4, 5, 4, 0, 5};
    private List<Button>[] BUTTON_LISTS = new List<Button>[4]{ new List<Button>(), new List<Button>(), new List<Button>(), new List<Button>()};

    public List<GameObject> phaser_display_canvases = null;
    public List<GameObject> phaser_coverups = null;
    public List<GameObject> phaser_sliders = null;
    private float[] phaser_powers = new float[2]{0.0f, 0.0f};
    private Vector3[] phaser_slider_initial_positions = new Vector3[2];
    private Vector3 phaser_slide_direction = new Vector3(0f, 0.09666f, -0.2351f);

    public GameObject long_range_lever = null;
    public GameObject long_range_indicator = null;
    private float long_range_angle = 0.0f;

    public List<GameObject> phaser_switches = null;
    public GameObject phaser_switch_canvas;
    private float[] phaser_cooldowns = { 0.0f, 0.0f, 0.0f };
    private bool[] phaser_is_cooling_down = { false, false, false };
    private bool[] phaser_prev_is_enabled = { false, false, false };
    private float[] phaser_is_enabled = {1.0f, 1.0f, 1.0f}; //phaser enabled is binary, 0-1 used to go left to right

    private List<KeyCode> keys_down = new List<KeyCode>();

    private List<string> ray_targets = new List<string> {"short_range_phasers", "long_range_phasers", "long_range_direction", "phaser_switches"};
    private int target_index = 0;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);

        for (int i = 0; i < 2; i++)
        {
            phaser_slider_initial_positions[i] = phaser_sliders[i].transform.localPosition;
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        }

        BUTTON_LISTS[2].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[0], true, false));
        BUTTON_LISTS[2].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[1], true, false));

        BUTTON_LISTS[3].Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[2], true, true));
        BUTTON_LISTS[3].Add(new Button(CONTROL_DESCS[5], CONTROL_INDEXES[3], true, true));
        BUTTON_LISTS[3].Add(new Button(CONTROL_DESCS[6], CONTROL_INDEXES[4], true, true));
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        hud_info.setTitle(CONTROL_NAMES[index]);
        hud_info.setButtons(BUTTON_LISTS[index]);
        return hud_info;
    }

    private void displayAdjustment()
    {
        for (int i = 0; i < 2; i++)
        {
            //move physical slider
            phaser_sliders[i].transform.localPosition =
                new Vector3(phaser_slider_initial_positions[i].x,
                            phaser_slider_initial_positions[i].y + phaser_slide_direction.y * phaser_powers[i],
                            phaser_slider_initial_positions[i].z + phaser_slide_direction.z * phaser_powers[i]);

            //adjust screen
            int power_as_int = (int)(phaser_powers[i] * 100.0f);
            if (power_as_int < 100)
            {
                int position = (power_as_int / 5);
                if (i == 0)
                {
                    phaser_display_canvases[i].transform.GetChild(position + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.5f + (0.5f * (position / 20.0f)), 0.5f, 0f, (0.2f * (power_as_int % 5)));
                }
                else
                {
                    phaser_display_canvases[i].transform.GetChild(position + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0f, 0.5f + 0.34f * ((19 - position) / 20.0f), 1f, (0.2f * (power_as_int % 5)));
                }
            }
        }

        long_range_indicator.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, long_range_angle);
        long_range_lever.transform.localRotation = Quaternion.Euler(-69.416f, 0.0f, -90f + long_range_angle);

        for (int i = 0; i < 3; i++)
        {
            phaser_switches[i].transform.GetChild(0).localRotation = Quaternion.Euler(0, 22f + phaser_is_enabled[i] * -44f, 0f);
            phaser_switch_canvas.transform.GetChild(2 + (2 * i)).gameObject.GetComponent<UnityEngine.UI.Image>().fillAmount = phaser_is_enabled[i];
            phaser_coverups[i].SetActive(phaser_is_enabled[i] != 1.0f);
        }
    }
    void Update()
    {
        bool display_necessary = false;
        if (target_index < 2) //looking at either short-range phasers or long-range phasers
        {
            int power_direction = 0;
            if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow))
            {
                power_direction = 1;
            }
            if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))
            {
                power_direction -= 1;
            }
            if (power_direction != 0)
            {
                if (power_direction > 0)
                {
                    phaser_powers[target_index] = Mathf.Min(1.0f, phaser_powers[target_index] + Time.deltaTime * 0.1f);
                }
                else
                {
                    phaser_powers[target_index] = Mathf.Max(0.0f, phaser_powers[target_index] - Time.deltaTime * 0.1f);
                }
                if (phaser_powers[target_index] >= 1.0f)
                {
                    BUTTON_LISTS[target_index][1].updateInteractable(false);
                }
                else
                {
                    BUTTON_LISTS[target_index][1].updateInteractable(true);
                }
                if (phaser_powers[target_index] <= 0.0f)
                {
                    BUTTON_LISTS[target_index][0].updateInteractable(false);
                }
                else
                {
                    BUTTON_LISTS[target_index][0].updateInteractable(true);
                }
                display_necessary = true;
            }
        }
        else if (target_index == 2)
        {
            int angle_direction = 0;
            if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow))
            {
                angle_direction = 1;
            }
            if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))
            {
                angle_direction -= 1;
            }
            if (angle_direction != 0)
            {
                if (angle_direction > 0)
                {
                    long_range_angle += Time.deltaTime * 10f;
                }
                else
                {
                    long_range_angle -= Time.deltaTime * 10f;
                }
                if (long_range_angle > 360.0f)
                {
                    long_range_angle -= 360.0f;
                }
                else if (long_range_angle < 0.0f)
                {
                    long_range_angle += 360.0f;
                }
                display_necessary = true;
            }
        }
        else
        {
            KeyCode[] keys_to_check = new KeyCode[] {KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.LeftArrow, KeyCode.UpArrow,  KeyCode.RightArrow};
            for (int i = 0; i <= 2; i++)
            {
                if (phaser_is_cooling_down[i] == false)
                {
                    if (keys_down.Contains(keys_to_check[i]) || keys_down.Contains(keys_to_check[i + 3]))
                    {
                        phaser_is_cooling_down[i] = true;
                        phaser_cooldowns[i] = 1.0f;
                        BUTTON_LISTS[3][i].toggle();
                        BUTTON_LISTS[3][i].updateInteractable(false);
                    }
                }
            }
        }
        for (int i = 0; i <= 2; i++)
        {
            if (phaser_is_cooling_down[i] == true)
            {
                phaser_cooldowns[i] = Mathf.Max(0.0f, phaser_cooldowns[i] - Time.deltaTime);
                if (phaser_prev_is_enabled[i] == false) //turning off
                {
                    phaser_is_enabled[i] = phaser_cooldowns[i];
                }
                else //turning on
                {
                    phaser_is_enabled[i] = 1.0f - phaser_cooldowns[i];
                }
                if (phaser_cooldowns[i] <= 0.0f)
                {
                    phaser_prev_is_enabled[i] = !phaser_prev_is_enabled[i];
                    phaser_is_cooling_down[i] = false;
                    BUTTON_LISTS[3][i].untoggle();
                    BUTTON_LISTS[3][i].updateInteractable(true);
                }
                display_necessary = true;
            }
        }
        if (display_necessary == true)
        {
            displayAdjustment();
        }
        keys_down.Clear();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        target_index = ray_targets.IndexOf(current_target.name);
        keys_down = inputs;
    }
}
