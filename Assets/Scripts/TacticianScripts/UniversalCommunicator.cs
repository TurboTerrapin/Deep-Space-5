/*
    UniversalCommunicator.cs
    - Handles inputs for communicator keyboard
    - Displays to code screen
    Contributor(s): Jake Schott
    Last Updated: 4/29/2025
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UniversalCommunicator : MonoBehaviour, IControllable
{
    //CLASS CONSTANTS
    Color[] COLOR_OPTIONS = new Color[4] { new Color(0f, 0.84f, 1f), new Color(0.129f, 1f, 0.04f), new Color(0.69f, 0f, 0.69f), new Color(0.84f, 0.62f, 0f) };

    private string[] CONTROL_NAMES = new string[] {"CHARACTER INPUT", "COLOR SELECTOR", "CHARACTER CONFIGURATION", "RESET DISPLAY", "INPUT/OUTPUT TOGGLE"};
    private List<string> CONTROL_DESCS = new List<string> { "INPUT", "SHIFT LEFT", "SHIFT RIGHT", "SWITCH", "RESET", "FLIP"};
    private List<int> CONTROL_INDEXES = new List<int>() {6, 4, 5, 6, 6, 6};
    private List<Button>[] BUTTON_LISTS = new List<Button>[5];

    private bool cooling_down = false;
    private float cooldown_timer = 0.0f;

    public List<GameObject> character_displays = null;
    public GameObject code_display;
    public List<GameObject> character_buttons = null;
    private Vector3[] character_button_initial_positions = new Vector3[12];
    private Vector3 character_button_push_direction = new Vector3(0, -0.0024f, -0.0009f);

    private List<int> code_index = new List<int>(); //0-11, corresponds to A0-A5, B0-B5 where B5 is 11 and A0 is 0
    private List<bool> code_is_numeric = new List<bool>(); //true is number (ex. 5), false is symbol (ex. square)
    private List<int> code_color = new List<int>(); //0 is blue, 1 is green, 2 is pink, 3 is orange

    public GameObject color_selector_slider = null;
    private float prev_color_selector_x_dist = -0.412f;
    private float color_selector_x_dist = -0.081f;

    public GameObject numeric_selector = null;
    public GameObject numeric_indicator_display = null;
    private float numeric_selector_x_dist = -0.0356f;

    public GameObject reset_button = null;
    private Vector3 reset_button_initial_position;
    private Vector3 reset_button_push_direction = new Vector3(0, -0.0049f, -0.0018f);

    private int currently_updating = -1;
    private int selection_index = -1;
    private int curr_color = 0;
    private bool curr_numeric = true;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private List<string> ray_targets = new List<string> {"A0", "A1", "A2", "A3", "A4", "A5", "B0", "B1", "B2", "B3", "B4", "B5", "color_selector", "numeric_selector", "reset_button", "input_output_toggle"};
    private int target_index = 0;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);

        BUTTON_LISTS[0] = new List<Button>();
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        for (int i = 0; i < 12; i++)
        {
            character_button_initial_positions[i] = character_buttons[i].transform.localPosition;
        }

        BUTTON_LISTS[1] = new List<Button>();
        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], false, true));
        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], true, true));

        BUTTON_LISTS[2] = new List<Button>();
        BUTTON_LISTS[2].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[3], true, true));

        BUTTON_LISTS[3] = new List<Button>();
        BUTTON_LISTS[3].Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[4], true, true));
        reset_button_initial_position = reset_button.transform.localPosition;

        BUTTON_LISTS[4] = new List<Button>();
        BUTTON_LISTS[4].Add(new Button(CONTROL_DESCS[5], CONTROL_INDEXES[5], false, false));
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        if (index < 12)
        {
            hud_info.setTitle(CONTROL_NAMES[0]);
            hud_info.setButtons(BUTTON_LISTS[0]);
        }
        else if (index == 12)
        {
            hud_info.setTitle(CONTROL_NAMES[1]);
            hud_info.setButtons(BUTTON_LISTS[1]);
        }
        else if (index == 13)
        {
            hud_info.setTitle(CONTROL_NAMES[2]);
            hud_info.setButtons(BUTTON_LISTS[2]);
        }
        else if (index == 14)
        {
            hud_info.setTitle(CONTROL_NAMES[3]);
            hud_info.setButtons(BUTTON_LISTS[3]);
        }
        else if (index == 15)
        {
            hud_info.setTitle(CONTROL_NAMES[4]);
            hud_info.setButtons(BUTTON_LISTS[4]);
        }

        return hud_info;
    }
    private void displayAdjustment(int to_update)
    {
        if (to_update == 1) { 
            code_display.transform.GetChild(25).transform.localPosition = new Vector3(0.14f - (code_index.Count * 0.04f), 0.0283f, 0f);
            if (code_index.Count >= 8)
            {
                code_display.transform.GetChild(25).gameObject.SetActive(false);
            }
            for (int i = 0; i < code_index.Count; i++)
            {
                if (code_is_numeric[i] == true) //is a number
                {
                    string char_text = character_displays[code_index[i]].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text;
                    code_display.transform.GetChild(i + 1).gameObject.GetComponent<TMP_Text>().SetText(char_text);
                    code_display.transform.GetChild(i + 1).gameObject.GetComponent<TMP_Text>().color = COLOR_OPTIONS[code_color[i]];
                    code_display.transform.GetChild(i + 1).gameObject.SetActive(true);
                }
                else //is a symbol
                {
                    code_display.transform.GetChild(i + 9).gameObject.GetComponent<UnityEngine.UI.RawImage>().texture = character_displays[code_index[i]].transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.RawImage>().mainTexture;
                    code_display.transform.GetChild(i + 9).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = COLOR_OPTIONS[code_color[i]];
                    code_display.transform.GetChild(i + 9).gameObject.SetActive(true);
                }
            }
            if (cooling_down == true)
            {
                float push_distance = 1f - ((cooldown_timer - 0.125f) / 0.125f);
                if (cooldown_timer <= 0.125f)
                {
                    push_distance = (cooldown_timer / 0.125f);
                }
                character_buttons[selection_index].transform.localPosition =
                    new Vector3(character_button_initial_positions[selection_index].x,
                                character_button_initial_positions[selection_index].y + push_distance * character_button_push_direction.y,
                                character_button_initial_positions[selection_index].z + push_distance * character_button_push_direction.z);
            }
        }
        else if (to_update == 2)
        {
            float desired_x_pos = -0.412f + color_selector_x_dist * (curr_color / 3.0f);
            float move_direction = -1f;
            if (color_selector_slider.transform.localPosition.x > desired_x_pos)
            {
                move_direction = 1;
            }

            if (cooldown_timer <= 0.0f)
            {
                color_selector_slider.transform.localPosition =
                    new Vector3(desired_x_pos,
                                color_selector_slider.transform.localPosition.y,
                                color_selector_slider.transform.localPosition.z);
                for (int i = 0; i < 12; i++)
                {
                    character_displays[i].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().color = COLOR_OPTIONS[curr_color];
                    character_displays[i].transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = COLOR_OPTIONS[curr_color];
                }
            }
            else
            {
                color_selector_slider.transform.localPosition =
                    new Vector3(prev_color_selector_x_dist + move_direction * ((0.5f - cooldown_timer) / 0.5f) * (color_selector_x_dist / 3.0f),
                                color_selector_slider.transform.localPosition.y,
                                color_selector_slider.transform.localPosition.z);
            }
        }
        else if (to_update == 3)
        {
            float x_pos = numeric_selector_x_dist;
            if (cooldown_timer <= 0.0f)
            {
                if (curr_numeric == true)
                {
                    x_pos = 0.0f;
                }
                numeric_selector.transform.localPosition =
                    new Vector3(
                                x_pos,
                                numeric_selector.transform.localPosition.y,
                                numeric_selector.transform.localPosition.z);
                for (int i = 0; i < 12; i++)
                {
                    character_displays[i].transform.GetChild(1).gameObject.SetActive(curr_numeric);
                    character_displays[i].transform.GetChild(2).gameObject.SetActive(!curr_numeric);
                }
                numeric_indicator_display.transform.GetChild(1).gameObject.SetActive(curr_numeric);
                numeric_indicator_display.transform.GetChild(2).gameObject.SetActive(!curr_numeric);
            }
            else
            {
                float percent_to_max = cooldown_timer / 0.5f;
                if (curr_numeric == false)
                {
                    percent_to_max = 1.0f - percent_to_max;
                }
                numeric_selector.transform.localPosition =
                    new Vector3(
                                percent_to_max * numeric_selector_x_dist,
                                numeric_selector.transform.localPosition.y,
                                numeric_selector.transform.localPosition.z);
            }
        }
        else if (to_update == 4)
        {
            code_display.transform.GetChild(25).gameObject.SetActive(true);
            code_display.transform.GetChild(25).transform.localPosition = new Vector3(0.14f, 0.0283f, 0f);
            for (int i = 0; i < 8; i++)
            {
                code_display.transform.GetChild(i + 1).gameObject.SetActive(false);
                code_display.transform.GetChild(i + 9).gameObject.SetActive(false);
            }
            float push_distance = 1f - ((cooldown_timer - 0.25f) / 0.25f);
            if (cooldown_timer <= 0.25f)
            {
                push_distance = (cooldown_timer / 0.25f);
            }
            reset_button.transform.localPosition =
                new Vector3(reset_button_initial_position.x,
                            reset_button_initial_position.y + push_distance * reset_button_push_direction.y,
                            reset_button_initial_position.z + push_distance * reset_button_push_direction.z);
        }
    }
    void Update()
    {
        if (cooling_down == false)
        {
            if (target_index < 12)
            {
                if (keys_down.Contains(KeyCode.Mouse0) || keys_down.Contains(KeyCode.KeypadEnter))
                {
                    cooldown_timer = 0.25f;
                    cooling_down = true;
                    selection_index = target_index;
                    code_index.Add(selection_index);
                    code_color.Add(curr_color);
                    code_is_numeric.Add(curr_numeric);
                    BUTTON_LISTS[0][0].toggle();
                    BUTTON_LISTS[0][0].updateInteractable(false);
                    currently_updating = 1;
                    displayAdjustment(currently_updating);
                }
            }
            else if (target_index == 12)
            {
                int slider_direction = 0;
                if (curr_color != 3 && (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow))) //E to increment
                {
                    slider_direction = 1;
                    BUTTON_LISTS[1][1].toggle();
                }
                else if (curr_color != 0 && (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow)))  //Q to decrement
                {
                    slider_direction = -1;
                    BUTTON_LISTS[1][0].toggle();
                }
                if (slider_direction != 0)
                {
                    cooling_down = true;
                    cooldown_timer = 0.5f;
                    curr_color += slider_direction;
                    BUTTON_LISTS[1][0].updateInteractable(false);
                    BUTTON_LISTS[1][1].updateInteractable(false);
                    prev_color_selector_x_dist = color_selector_slider.transform.localPosition.x;
                    currently_updating = 2;
                    displayAdjustment(currently_updating);
                }
            }
            else if (target_index == 13)
            {
                if (keys_down.Contains(KeyCode.Mouse0) || keys_down.Contains(KeyCode.KeypadEnter))
                {
                    cooldown_timer = 0.5f;
                    cooling_down = true;
                    BUTTON_LISTS[2][0].toggle();
                    BUTTON_LISTS[2][0].updateInteractable(false);
                    curr_numeric = !curr_numeric;
                    currently_updating = 3;
                    displayAdjustment(currently_updating);
                }
            }
            else if (target_index == 14)
            {
                if (keys_down.Contains(KeyCode.Mouse0) || keys_down.Contains(KeyCode.KeypadEnter))
                {
                    cooldown_timer = 0.5f;
                    cooling_down = true;
                    BUTTON_LISTS[3][0].toggle();
                    BUTTON_LISTS[3][0].updateInteractable(false);
                    code_index.Clear();
                    code_color.Clear();
                    code_is_numeric.Clear();
                    currently_updating = 4;
                    displayAdjustment(currently_updating);
                }
            }
        }
        else
        {
            cooldown_timer = Mathf.Max(0.0f, cooldown_timer - Time.deltaTime);
            displayAdjustment(currently_updating);
            if (cooldown_timer <= 0.0f)
            {
                cooling_down = false;
                BUTTON_LISTS[0][0].untoggle();
                if (code_index.Count < 8)
                {
                    BUTTON_LISTS[0][0].updateInteractable(true);
                }

                if (curr_color != 3)
                {
                    BUTTON_LISTS[1][1].updateInteractable(true);
                }
                else
                {
                    BUTTON_LISTS[1][1].untoggle();
                    BUTTON_LISTS[1][1].updateInteractable(false);
                }
                if (curr_color != 0)
                {
                    BUTTON_LISTS[1][0].updateInteractable(true);
                }
                else
                {
                    BUTTON_LISTS[1][0].untoggle();
                    BUTTON_LISTS[1][0].updateInteractable(false);
                }

                BUTTON_LISTS[2][0].untoggle();
                BUTTON_LISTS[2][0].updateInteractable(true);

                BUTTON_LISTS[3][0].untoggle();
                BUTTON_LISTS[3][0].updateInteractable(true);
            }
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        target_index = ray_targets.IndexOf(current_target.name);
        keys_down = inputs;
    }
}
