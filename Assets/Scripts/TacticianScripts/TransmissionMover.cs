/*
    TransmissionMover.cs
    - Moves the waves
    Contributor(s): Jake Schott
    Last Updated: 4/15/2025
*/

using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TransmissionMover : MonoBehaviour, IControllable
{
    private string[] CONTROL_NAMES = new string[] {"FREQUENCY ADJUSTER", "SIGNAL OPTIONS"};
    private List<string> CONTROL_DESCS = new List<string> {"SLOW", "PREVIOUS", "NEXT", "RECEIVE", "BROADCAST"};
    private List<int> CONTROL_INDEXES = new List<int>() {6, 4, 5, 2, 0};
    private List<Button>[] BUTTON_LISTS = new List<Button>[2] { new List<Button>(), new List<Button>()};

    private List<KeyCode> keys_down = new List<KeyCode>();

    public GameObject transmission_canvas;
    public GameObject frequency_text;
    public GameObject success_light;
    public List<GameObject> waves = null;
    public List<GameObject> directional_buttons = null;
    public List<GameObject> signal_a_list = null;
    public List<GameObject> signal_b_list = null;

    public Material unlit_blue;
    public Material neon;
    public Material lit_red;
    public Material unlit_red;
    public Material lit_green;
    public Material unlit_green;

    private Vector3 frequency_push_direction = new Vector3(-0.0013f, -0.0036f, 0f);
    private bool fa_cooling_down = false;
    private float fa_cool_down_timer = 0.0f;
    private int fa_last_pressed = 1; //1 is previous, 2 is next
    private Vector3 signal_push_direction = new Vector3(-0.0019f, -0.0053f, 0f);
    private bool signal_cooling_down = false;
    private float signal_cool_down_timer = 0.0f;
    private int signal_last_pressed = 1;
    private int signal_stage = 1;
    private bool light_switch = false;

    private List<string> frequencies = new List<string>(){"120.5", "126.1", "129.4", "129.8", "134.3", "139.9"};
    private List<int> corresponding_waves = new List<int>() { 4, 4, 4, 4, 5, 4 };
    private int frequency_index = 0;

    public GameObject slow_button;
    private float slow_button_push = 0.0f; //0 is not pushed at all, 1 is fully pushed
    private Vector3 slow_button_final_direction = new Vector3(-0.0019f, -0.0052f, 0f);
    private float move_speed = 1.0f;
    private Vector3[] initial_positions = new Vector3[5];

    private List<string> ray_targets = new List<string> {"frequency_adjuster", "signal_options"};
    private int target_index = 0;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        initial_positions[0] = directional_buttons[0].transform.localPosition;
        initial_positions[1] = directional_buttons[1].transform.localPosition;
        initial_positions[2] = directional_buttons[2].transform.localPosition;
        initial_positions[3] = directional_buttons[3].transform.localPosition;
        initial_positions[4] = slow_button.transform.localPosition;

        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, true));
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], true, true));

        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[3], true, true));
        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[4], true, true));
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        hud_info.setTitle(CONTROL_NAMES[index]);
        hud_info.setButtons(BUTTON_LISTS[index]);
        return hud_info;
    }
    private void turnoffSignalLights()
    {
        for (int i = 0; i < 5; i++)
        {
            signal_a_list[i].GetComponent<Renderer>().material = unlit_blue;
        }
        for (int i = 0; i < 5; i++)
        {
            signal_b_list[i].GetComponent<Renderer>().material = unlit_blue;
        }
    }
    private void displayAdjustment()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].GetComponent<UnityEngine.UI.RawImage>().texture = transmission_canvas.transform.GetChild(corresponding_waves[frequency_index]).gameObject.GetComponent<UnityEngine.UI.RawImage>().mainTexture;
            waves[i].GetComponent<RectTransform>().localPosition -= new Vector3(0f, 0.05f * Time.deltaTime * move_speed, 0f);
        }
        if (waves[0].GetComponent<RectTransform>().localPosition.y <= -0.109f)
        {
            waves[0].GetComponent<RectTransform>().localPosition = new Vector3(0f, 0.114f, 0f);
            GameObject temp_wave = waves[0];
            waves.RemoveAt(0);
            waves.Add(temp_wave);
        }
        slow_button.transform.localPosition =
            new Vector3(initial_positions[4].x + slow_button_push * slow_button_final_direction.x,
                        initial_positions[4].y + slow_button_push * slow_button_final_direction.y,
                        initial_positions[4].z);
        frequency_text.GetComponent<TMP_Text>().SetText(frequencies[frequency_index] + "Mh");
        if (fa_cooling_down == true)
        {
            float push_distance = 1f - ((fa_cool_down_timer - 0.125f) / 0.125f);
            if (fa_cool_down_timer <= 0.125f)
            {
                push_distance = (fa_cool_down_timer / 0.125f);
            }
            directional_buttons[fa_last_pressed - 1].transform.localPosition =
                new Vector3(initial_positions[fa_last_pressed - 1].x + push_distance * frequency_push_direction.x,
                            initial_positions[fa_last_pressed - 1].y + push_distance * frequency_push_direction.y,
                            initial_positions[fa_last_pressed - 1].z);
        }
        if (signal_cooling_down == true)
        {
            if (signal_stage == 1)
            {
                float push_distance = 1f - ((signal_cool_down_timer - 0.125f) / 0.125f);
                if (signal_cool_down_timer <= 0.125f)
                {
                    push_distance = (signal_cool_down_timer / 0.125f);
                }
                directional_buttons[signal_last_pressed - 1].transform.localPosition =
                    new Vector3(initial_positions[signal_last_pressed - 1].x + push_distance * signal_push_direction.x,
                                initial_positions[signal_last_pressed - 1].y + push_distance * signal_push_direction.y,
                                initial_positions[signal_last_pressed - 1].z);
            }
            else if (signal_stage < 5)
            {
                turnoffSignalLights();
                int to_illuminate = (int)(signal_cool_down_timer * 100) / 20;
                if (light_switch)
                {
                    signal_a_list[to_illuminate].GetComponent<Renderer>().material = neon;
                    signal_b_list[4 - to_illuminate].GetComponent<Renderer>().material = neon;
                }
                else
                {
                    signal_a_list[4 - to_illuminate].GetComponent<Renderer>().material = neon;
                    signal_b_list[to_illuminate].GetComponent<Renderer>().material = neon;
                }
            }
            else
            {
                turnoffSignalLights();
                success_light.GetComponent<Renderer>().material = lit_green;
            }
        }
        else
        {
            success_light.GetComponent<Renderer>().material = unlit_green;
            turnoffSignalLights();
        }
    }
    void Update()
    {
        if (target_index == 0) //looking at frequency adjuster
        {
            if (keys_down.Contains(KeyCode.Mouse0) || keys_down.Contains(KeyCode.KeypadEnter))
            {
                slow_button_push = Mathf.Min(1.0f, slow_button_push + Time.deltaTime * 5.0f);
                move_speed = 0.25f;
            }
            else
            {
                slow_button_push = Mathf.Max(0.0f, slow_button_push - Time.deltaTime * 5.0f);
                move_speed = 1.0f;
            }
            if (fa_cooling_down == false)
            {
                bool adjusted_frequency = false;
                if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow))
                {
                    adjusted_frequency = true;
                    fa_last_pressed = 2;
                    frequency_index++;
                    if (frequency_index >= frequencies.Count - 1)
                    {
                        frequency_index = 0;
                    }
                }
                else if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))
                {
                    adjusted_frequency = true;
                    fa_last_pressed = 1;
                    frequency_index--;
                    if (frequency_index < 0)
                    {
                        frequency_index = frequencies.Count - 1;
                    }
                }
                if (adjusted_frequency == true)
                {
                    fa_cooling_down = true;
                    fa_cool_down_timer = 0.25f;
                    BUTTON_LISTS[0][fa_last_pressed].toggle(0.2f);
                    BUTTON_LISTS[0][1].updateInteractable(false);
                    BUTTON_LISTS[0][2].updateInteractable(false);
                }
            }
        }
        else //looking at signal options
        {
            if (signal_cooling_down == false)
            {
                bool signal_adjusted = false;
                if (keys_down.Contains(KeyCode.S) || keys_down.Contains(KeyCode.DownArrow))
                {
                    signal_adjusted = true;
                    signal_last_pressed = 3;
                }
                else if (keys_down.Contains(KeyCode.W) || keys_down.Contains(KeyCode.UpArrow))
                {
                    signal_adjusted = true;
                    signal_last_pressed = 4;
                }
                if (signal_adjusted == true)
                {
                    signal_cooling_down = true;
                    signal_cool_down_timer = 0.25f;
                    signal_stage = 1;
                    light_switch = true;
                    BUTTON_LISTS[1][signal_last_pressed - 3].toggle(0.2f);
                    BUTTON_LISTS[1][0].updateInteractable(false);
                    BUTTON_LISTS[1][1].updateInteractable(false);
                }
            }
            else
            {
                if (signal_cool_down_timer <= 0.0f)
                {
                    signal_stage++;
                    if (signal_stage < 5)
                    {
                        signal_cool_down_timer = 1.0f;
                        light_switch = !light_switch;
                    }
                    else if (signal_stage == 5)
                    {
                        signal_cool_down_timer = 1.0f;
                    }
                    else
                    {
                        BUTTON_LISTS[1][0].updateInteractable(true);
                        BUTTON_LISTS[1][1].updateInteractable(true);
                        signal_cooling_down = false;
                    }
                }
            }
        }
        if (fa_cooling_down == true)
        {
            fa_cool_down_timer = Mathf.Max(0.0f, fa_cool_down_timer - Time.deltaTime);
            if (fa_cool_down_timer <= 0.0f)
            {
                BUTTON_LISTS[0][1].updateInteractable(true);
                BUTTON_LISTS[0][2].updateInteractable(true);
                fa_cooling_down = false;
            }
        }
        if (signal_cooling_down == true) 
        {
            signal_cool_down_timer = Mathf.Max(0.0f, signal_cool_down_timer - Time.deltaTime);
        }
        displayAdjustment();
        keys_down.Clear();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        target_index = ray_targets.IndexOf(current_target.name);
        keys_down = inputs;
    }
}
