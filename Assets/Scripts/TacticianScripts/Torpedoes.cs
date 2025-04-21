/*
    Torpedoes.cs
    - Handles inputs for all four torpedo bays
    - Moves levers accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/21/2025
*/

using UnityEngine;
using System.Collections.Generic;
public class Torpedoes : MonoBehaviour, IControllable
{
    //CLASS CONSTANTS
    private float MOVE_SPEED = 75.0f;

    private string[] CONTROL_NAMES = new string[] { "FORWARD TORPEDO POWER", "PORT TORPEDO POWER", "STARBOARD TORPEDO POWER", "AFT TORPEDO POWER", "TORPEDO SELECTOR"};
    private List<string> CONTROL_DESCS = new List<string> {"REDUCE", "ENERGIZE", "SHIFT LEFT", "SHIFT RIGHT"};
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button>[] BUTTON_LISTS = new List<Button>[5];

    public GameObject torpedo_selector_lever;
    public GameObject torpedo_selector_canvas;
    private bool torpedo_cooling_down = false;
    private float torpedo_cooldown_timer = 0.0f;
    private int torpedo_option = 0;
    private float prev_torpedo_selector_x_dist = 0.0f;
    private float torpedo_selector_x_dist = -0.08193f;

    public List<GameObject> levers = null;
    public List<GameObject> information_containers = null; //contains screens and indicators

    private List<KeyCode> keys_down = new List<KeyCode>();

    private List<string> ray_targets = new List<string> { "forward_torpedo_power", "port_torpedo_power", "starboard_torpedo_power", "aft_torpedo_power", "torpedo_selector"};
    private int target_index = 0;
    private float[] power_levels = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
    private int power_direction = 0;
    private Vector3[] initial_positions = new Vector3[4]; //handle starting position (0% power)
    private Vector3 final_pos = new Vector3(-0.0754f, 0.0141f, 0f); //handle final position (100% power)

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        for (int i = 0; i <= 3; i++)
        {
            BUTTON_LISTS[i] = new List<Button>();
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
            initial_positions[i] = levers[i].transform.position;
        }

        BUTTON_LISTS[4] = new List<Button>();
        BUTTON_LISTS[4].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[0], false, true));
        BUTTON_LISTS[4].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[1], true, true));
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
            levers[index].transform.position =
                new Vector3(initial_positions[index].x + ((final_pos.x - initial_positions[index].x) * power_levels[index]),
                            initial_positions[index].y + ((final_pos.y - initial_positions[index].y) * power_levels[index]),
                            initial_positions[index].z); //no changes in z
        }
        else
        {

            float desired_x_pos = torpedo_selector_x_dist * (torpedo_option / 3.0f);
            float move_direction = -1f;
            if (torpedo_selector_lever.transform.localPosition.x > desired_x_pos)
            {
                move_direction = 1;
            }

            if (torpedo_cooldown_timer <= 0.0f)
            {
                torpedo_selector_lever.transform.localPosition =
                    new Vector3(desired_x_pos,
                                torpedo_selector_lever.transform.localPosition.y,
                                torpedo_selector_lever.transform.localPosition.z);
                torpedo_selector_canvas.transform.GetChild(torpedo_option + 5).gameObject.SetActive(true);
            }
            else
            {
                for (int i = 5; i <= 8; i++)
                {
                    torpedo_selector_canvas.transform.GetChild(i).gameObject.SetActive(false);
                }
                torpedo_selector_lever.transform.localPosition =
                    new Vector3(prev_torpedo_selector_x_dist + move_direction * ((0.5f - torpedo_cooldown_timer) / 0.5f) * (torpedo_selector_x_dist / 3.0f),
                                torpedo_selector_lever.transform.localPosition.y,
                                torpedo_selector_lever.transform.localPosition.z);
            }
        }
    }
    void Update()
    {
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
        else
        {
            if (torpedo_cooling_down == false)
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
                    torpedo_cooling_down = true;
                    torpedo_cooldown_timer = 0.5f;
                    torpedo_option += torpedo_direction;
                    BUTTON_LISTS[4][0].updateInteractable(false);
                    BUTTON_LISTS[4][1].updateInteractable(false);
                    prev_torpedo_selector_x_dist = torpedo_selector_lever.transform.localPosition.x;
                    displayAdjustment(4);
                }
            }
            else
            {
                torpedo_cooldown_timer = Mathf.Max(0.0f, torpedo_cooldown_timer - Time.deltaTime);
                if (torpedo_cooldown_timer <= 0.0f)
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
                    torpedo_cooling_down = false;
                }
                displayAdjustment(4);
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
