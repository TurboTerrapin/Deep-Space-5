/*
    Torpedoes.cs
    - Handles inputs for all four torpedo bays
    - Moves power_levers accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/22/2025
*/

using UnityEngine;
using System.Collections.Generic;
public class Torpedoes : MonoBehaviour, IControllable
{
    //CLASS CONSTANTS
    private float MOVE_SPEED = 75.0f;

    private string[] CONTROL_NAMES = new string[] { "FORWARD TORPEDO POWER", "PORT TORPEDO POWER", "STARBOARD TORPEDO POWER", "AFT TORPEDO POWER", "TORPEDO SELECTOR", "TORPEDO TRIGGER"};
    private List<string> CONTROL_DESCS = new List<string> {"REDUCE", "ENERGIZE", "SHIFT LEFT", "SHIFT RIGHT", "FIRE", "ARM"};
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5, 6, 11};
    private List<Button>[] BUTTON_LISTS = new List<Button>[6];

    public Material lit_red;
    public Material unlit_red;
    public Material lit_green;
    public Material unlit_green;

    public GameObject selector_lever;
    public GameObject selector_canvas;
    private bool selector_cooling_down = false;
    private float selector_cooldown_timer = 0.0f;
    private int torpedo_option = 0;
    private float prev_selector_x_dist = 0.0f;
    private float selector_x_dist = -0.08193f;

    public List<GameObject> power_levers = null;
    public List<GameObject> information_containers = null; //contains screens and indicators

    public GameObject trigger_base;
    public GameObject trigger_green_light;
    public GameObject trigger_red_light;
    private float trigger_percentage = 0.0f;
    private bool trigger_cooling_down = false;
    private float trigger_cooldown_timer = 0.0f;
    private Vector3 trigger_base_final_pos = new Vector3(0f, -0.0155f, 0.0369f);
    private Vector3 trigger_base_initial_pos;

    private List<string> ray_targets = new List<string> { "forward_torpedo_power", "port_torpedo_power", "starboard_torpedo_power", "aft_torpedo_power", "torpedo_selector", "torpedo_trigger"};
    private int target_index = 0;
    private float[] power_levels = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
    private int power_direction = 0;
    private Vector3[] initial_positions = new Vector3[4]; //handle starting position (0% power)
    private Vector3 final_pos = new Vector3(-0.0754f, 0.0141f, 0f); //handle final position (100% power)

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        for (int i = 0; i <= 3; i++)
        {
            BUTTON_LISTS[i] = new List<Button>();
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
            initial_positions[i] = power_levers[i].transform.position;
        }

        BUTTON_LISTS[4] = new List<Button>();
        BUTTON_LISTS[4].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[0], false, true));
        BUTTON_LISTS[4].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[1], true, true));

        trigger_base_initial_pos = trigger_base.transform.localPosition;
        BUTTON_LISTS[5] = new List<Button>();
        BUTTON_LISTS[5].Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[2], false, true));
        BUTTON_LISTS[5].Add(new Button(CONTROL_DESCS[5], CONTROL_INDEXES[3], true, false));
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        hud_info.setTitle(CONTROL_NAMES[index]);
        hud_info.setButtons(BUTTON_LISTS[index]);
        return hud_info;
    }
    private void displayAdjustment(int index)
    {
        if (index < 4)
        {
            //update bars on screen
            int power_as_int = (int)(power_levels[index] * 100.0f);
            if (power_as_int < 100)
            {
                for (int i = 0; i <= 1; i++)
                {
                    information_containers[index].transform.GetChild(i).GetChild((power_as_int / 5) + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.73f, 1.0f, (0.1f * (power_as_int % 5)));
                }
            }

            //update lit indicators
            information_containers[index].transform.GetChild(2).gameObject.SetActive(!(power_as_int == 100)); //unlit green
            information_containers[index].transform.GetChild(3).gameObject.SetActive(power_as_int == 100); //lit green
            information_containers[index].transform.GetChild(4).gameObject.SetActive(power_as_int == 100); //unlit red
            information_containers[index].transform.GetChild(5).gameObject.SetActive(!(power_as_int == 100)); //lit red

            //update lever position
            power_levers[index].transform.position =
                new Vector3(initial_positions[index].x + ((final_pos.x - initial_positions[index].x) * power_levels[index]),
                            initial_positions[index].y + ((final_pos.y - initial_positions[index].y) * power_levels[index]),
                            initial_positions[index].z); //no changes in z
        }
        else if (index == 4)
        {

            float desired_x_pos = selector_x_dist * (torpedo_option / 3.0f);
            float move_direction = -1f;
            if (selector_lever.transform.localPosition.x > desired_x_pos)
            {
                move_direction = 1;
            }

            if (selector_cooldown_timer <= 0.0f)
            {
                selector_lever.transform.localPosition =
                    new Vector3(desired_x_pos,
                                selector_lever.transform.localPosition.y,
                                selector_lever.transform.localPosition.z);
                selector_canvas.transform.GetChild(torpedo_option + 5).gameObject.SetActive(true);
            }
            else
            {
                for (int i = 5; i <= 8; i++)
                {
                    selector_canvas.transform.GetChild(i).gameObject.SetActive(false);
                }
                selector_lever.transform.localPosition =
                    new Vector3(prev_selector_x_dist + move_direction * ((0.5f - selector_cooldown_timer) / 0.5f) * (selector_x_dist / 3.0f),
                                selector_lever.transform.localPosition.y,
                                selector_lever.transform.localPosition.z);
            }
        }
        else if (index == 5)
        {
            float trigger_base_distance_percentage = Mathf.Min(1.0f, trigger_percentage / 0.8f);
            trigger_base.transform.localPosition =
                new Vector3(trigger_base_initial_pos.x,
                            trigger_base_initial_pos.y + trigger_base_distance_percentage * trigger_base_final_pos.y,
                            trigger_base_initial_pos.z + trigger_base_distance_percentage * trigger_base_final_pos.z);

            float trigger_lever_rotation = Mathf.Max(0.0f, (trigger_percentage - 0.5f) / 0.5f);
            trigger_base.transform.GetChild(0).localRotation = Quaternion.Euler(20f + (trigger_lever_rotation * 15f), 0f, 90f);

            float red_button_percentage = 0.0f;
            if (trigger_cooldown_timer > 2.5f)
            {
                red_button_percentage = 1f - ((trigger_cooldown_timer - 2.75f) / 0.25f);
                if (trigger_cooldown_timer <= 2.75f)
                {
                    red_button_percentage = ((trigger_cooldown_timer - 2.5f) / 0.25f);
                }
            }
            trigger_base.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0f, 0f, red_button_percentage * -0.00004f);

            if (trigger_percentage >= 1.0f)
            {
                trigger_green_light.GetComponent<Renderer>().material = lit_green;
                trigger_red_light.GetComponent<Renderer>().material = unlit_red;
            }
            else
            {
                trigger_green_light.GetComponent<Renderer>().material = unlit_green;
                trigger_red_light.GetComponent<Renderer>().material = lit_red;
            }
        }
    }
    void Update()
    {
        bool arming_torpedo = false;
        if (target_index < 4)
        {
            power_direction = 0;
            if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow)) //E to increment
            {
                power_direction += 1;
            }
            if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))  //Q to decrement
            {
                power_direction -= 1;
            }
            if (power_direction != 0)
            {
                if (power_direction > 0)
                {
                    power_levels[target_index] = Mathf.Min(1.0f, power_levels[target_index] + (0.002f * (power_levels[target_index] / 0.5f) + 0.001f) * Time.deltaTime * MOVE_SPEED);
                }
                else
                {
                    power_levels[target_index] = Mathf.Max(0.0f, power_levels[target_index] - (0.002f * (power_levels[target_index] / 0.5f) + 0.001f) * Time.deltaTime * MOVE_SPEED);
                }
                if (power_levels[target_index] <= 0f)
                {
                    BUTTON_LISTS[target_index][0].updateInteractable(false);
                }
                else
                {
                    BUTTON_LISTS[target_index][0].updateInteractable(true);
                }
                if (power_levels[target_index] >= 1f)
                {
                    BUTTON_LISTS[target_index][1].updateInteractable(false);
                }
                else
                {
                    BUTTON_LISTS[target_index][1].updateInteractable(true);
                }
                displayAdjustment(target_index);
            }
        }
        else if (target_index == 4)
        {
            if (selector_cooling_down == false)
            {
                int torpedo_direction = 0;
                if (torpedo_option != 3 && (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow))) //E to increment
                {
                    torpedo_direction = 1;
                    BUTTON_LISTS[4][1].toggle();
                }
                else if (torpedo_option != 0 && (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow)))  //Q to decrement
                {
                    torpedo_direction = -1;
                    BUTTON_LISTS[4][0].toggle();
                }
                if (torpedo_direction != 0)
                {
                    selector_cooling_down = true;
                    selector_cooldown_timer = 0.5f;
                    torpedo_option += torpedo_direction;
                    BUTTON_LISTS[4][0].updateInteractable(false);
                    BUTTON_LISTS[4][1].updateInteractable(false);
                    prev_selector_x_dist = selector_lever.transform.localPosition.x;
                    displayAdjustment(4);
                }
            }
            else
            {
                selector_cooldown_timer = Mathf.Max(0.0f, selector_cooldown_timer - Time.deltaTime);
                if (selector_cooldown_timer <= 0.0f)
                {
                    if (torpedo_option != 3)
                    {
                        BUTTON_LISTS[4][1].updateInteractable(true);
                    }
                    else
                    {
                        BUTTON_LISTS[4][1].untoggle();
                        BUTTON_LISTS[4][1].updateInteractable(false);
                    }
                    if (torpedo_option != 0)
                    {
                        BUTTON_LISTS[4][0].updateInteractable(true);
                    }
                    else
                    {
                        BUTTON_LISTS[4][0].untoggle();
                        BUTTON_LISTS[4][0].updateInteractable(false);
                    }
                    selector_cooling_down = false;
                }
                displayAdjustment(4);
            }
        }
        else if (target_index == 5)
        {
            if (trigger_cooling_down == false)
            {
                if (keys_down.Contains(KeyCode.F)) //F to arm
                {
                    arming_torpedo = true;
                    trigger_percentage = Mathf.Min(1.0f, trigger_percentage + Time.deltaTime);
                    if (trigger_percentage >= 1.0f)
                    {
                        BUTTON_LISTS[5][0].updateInteractable(true);
                    }
                }
                if (trigger_percentage >= 1.0f && (keys_down.Contains(KeyCode.Mouse0) || (keys_down.Contains(KeyCode.KeypadEnter))))
                {
                    trigger_cooldown_timer = 3.0f;
                    trigger_cooling_down = true;
                    BUTTON_LISTS[5][0].toggle();
                    BUTTON_LISTS[5][0].updateInteractable(false);
                    BUTTON_LISTS[5][1].updateInteractable(false);
                    displayAdjustment(5);
                }
                else
                {
                    if (arming_torpedo == true)
                    {
                        displayAdjustment(5);
                    }
                }
            }
        }
        if (arming_torpedo == false && trigger_percentage > 0.0f)
        {
            BUTTON_LISTS[5][0].updateInteractable(false);
            if (trigger_cooling_down == true)
            {
                trigger_cooldown_timer = Mathf.Max(0.0f, trigger_cooldown_timer - Time.deltaTime);
                trigger_percentage = trigger_cooldown_timer / 3f;
                if (trigger_cooldown_timer <= 0.0f)
                {
                    trigger_cooling_down = false;
                    trigger_percentage = 0.0f;
                    BUTTON_LISTS[5][1].updateInteractable(true);
                }
                else if (trigger_cooldown_timer <= 2.5f)
                {
                    BUTTON_LISTS[5][0].untoggle();
                    BUTTON_LISTS[5][0].updateInteractable(false);
                }
            }
            else
            {
                trigger_percentage = Mathf.Max(0.0f, trigger_percentage - Time.deltaTime);
            }
            displayAdjustment(5);
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        target_index = ray_targets.IndexOf(current_target.name);
        keys_down = inputs;
    }
}
