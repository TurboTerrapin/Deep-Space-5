/*
    Probe.cs
    - Handles probe orientation
    - Moves tractor beam lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/18/2025
*/


using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements.Experimental;

public class Probe : MonoBehaviour, IControllable
{
    private string[] CONTROL_NAMES = new string[] {"PROBE ORIENTATION", "PROBE LATERAL MOVEMENT", "PROBE VERTICAL MOVEMENT"};
    private List<string> CONTROL_DESCS = new List<string> {"TURN LEFT", "TURN RIGHT", "FORWARD", "LEFT", "REVERSE", "RIGHT", "DESCEND", "ASCEND"};
    private List<int> CONTROL_INDEXES = new List<int>() {4, 5, 0, 1, 2, 3, 2, 0};
    private List<Button>[] BUTTON_LISTS = new List<Button>[3] {new List<Button>(), new List<Button>(), new List<Button>()};

    public GameObject probe;

    public GameObject orientation_lever;
    public GameObject orientation_canvas;
    private float orientation_lever_angle = 0.0f;

    private float orientation_angle = 0.0f;

    public List<GameObject> lateral_buttons = null; //forward, left, down, right
    public GameObject lateral_canvas;
    private Vector3[] initial_lateral_positions = new Vector3[4];
    private float[] lateral_movement_factors = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f }; //forward, left, down, right
    private Vector3 lateral_button_move_direction = new Vector3(0, -0.006f, -0.0024f);
    private Vector3 probe_position;

    public GameObject vertical_lever;
    public GameObject vertical_canvas;
    private float vertical_lever_angle = 0.0f;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private List<string> ray_targets = new List<string> {"probe_orientation", "probe_lateral_movement", "probe_vertical_movement"};
    private int target_index = 0;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));

        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], true, false));
        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[3], true, false));
        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[4], true, false));
        BUTTON_LISTS[1].Add(new Button(CONTROL_DESCS[5], CONTROL_INDEXES[5], true, false));

        BUTTON_LISTS[2].Add(new Button(CONTROL_DESCS[6], CONTROL_INDEXES[6], true, false));
        BUTTON_LISTS[2].Add(new Button(CONTROL_DESCS[7], CONTROL_INDEXES[7], true, false));

        hud_info.setButtons(BUTTON_LISTS[0]);

        for (int i = 0; i < 4; i++)
        {
            initial_lateral_positions[i] = lateral_buttons[i].transform.localPosition;
        }
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
        //set orientation text
        string display_orientation = orientation_angle.ToString();
        if (!display_orientation.Contains("."))
        {
            display_orientation += ".0";
        }
        orientation_canvas.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().SetText(display_orientation);
        orientation_canvas.transform.GetChild(2).localPosition = new Vector3(0f, -0.012f + (display_orientation.Length - 3) * -0.004f, 0f);

        //update lever positions
        orientation_lever.transform.localRotation = Quaternion.Euler(-20f, -90f, orientation_lever_angle);
        vertical_lever.transform.localRotation = Quaternion.Euler(0f, -90f, -20f + vertical_lever_angle);

        //push lateral buttons, update circle
        for (int i = 0; i < 4; i++)
        {
            lateral_buttons[i].transform.localPosition =
                new Vector3(initial_lateral_positions[i].x,
                            initial_lateral_positions[i].y + lateral_movement_factors[i] * lateral_button_move_direction.y,
                            initial_lateral_positions[i].z + lateral_movement_factors[i] * lateral_button_move_direction.z
                            );
            lateral_canvas.transform.GetChild(i + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.83f, 1f, lateral_movement_factors[i]);
        }

        //update probe
        probe.transform.localRotation = Quaternion.Euler(0f, orientation_angle - 180f, 0f);
        probe.transform.localPosition = probe_position;
        vertical_canvas.transform.GetChild(1).transform.localPosition = new Vector3(-0.0095f, 0.045f * (probe.transform.localPosition.y / 50f), 0);
    }
    void Update()
    {
        int orientation_direction = 0; //-1 is turn left, 0 is no turn, 1 is turn right
        int horizontal_direction = 0; //-1 is move left, 0 is no movement, 1 is move right
        int forward_direction = 0; //-1 is move back, 0 is no movement, 1 is move forward
        int vertical_direction = 0; //-1 is down, 0 is no movement, 1 is move up

        //is looking at lateral movement
        if (target_index == 1)
        {
            //D to move right
            if (keys_down.Contains(KeyCode.D) || keys_down.Contains(KeyCode.RightArrow)) 
            {
                horizontal_direction += 1;
                lateral_movement_factors[3] = Mathf.Min(1.0f, lateral_movement_factors[3] + Time.deltaTime * 10.0f);
            }
            else
            {
                lateral_movement_factors[3] = Mathf.Max(0.0f, lateral_movement_factors[3] - Time.deltaTime * 10.0f);
            }

            //A to move left
            if (keys_down.Contains(KeyCode.A) || keys_down.Contains(KeyCode.LeftArrow))  
            {
                horizontal_direction -= 1;
                lateral_movement_factors[1] = Mathf.Min(1.0f, lateral_movement_factors[1] + Time.deltaTime * 10.0f);
            }
            else
            {
                lateral_movement_factors[1] = Mathf.Max(0.0f, lateral_movement_factors[1] - Time.deltaTime * 10.0f);
            }

            //W to move forward
            if (keys_down.Contains(KeyCode.W) || keys_down.Contains(KeyCode.UpArrow))
            {
                forward_direction += 1;
                lateral_movement_factors[0] = Mathf.Min(1.0f, lateral_movement_factors[0] + Time.deltaTime * 10.0f);
            }
            else
            {
                lateral_movement_factors[0] = Mathf.Max(0.0f, lateral_movement_factors[0] - Time.deltaTime * 10.0f);
            }

            //S to move backward
            if (keys_down.Contains(KeyCode.S) || keys_down.Contains(KeyCode.DownArrow))  
            {
                forward_direction -= 1;
                lateral_movement_factors[2] = Mathf.Min(1.0f, lateral_movement_factors[2] + Time.deltaTime * 10.0f);
            }
            else
            {
                lateral_movement_factors[2] = Mathf.Max(0.0f, lateral_movement_factors[2] - Time.deltaTime * 10.0f);
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                lateral_movement_factors[i] = Mathf.Max(0.0f, lateral_movement_factors[i] - Time.deltaTime * 10.0f);
            }
        }
        if (target_index == 0)
        {
            //E to turn right
            if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow))
            {
                orientation_direction += 1;
            }

            //Q to turn left
            if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))
            {
                orientation_direction -= 1;
            }
        }
        else if (target_index == 2)
        {
            //W to move up
            if (keys_down.Contains(KeyCode.W) || keys_down.Contains(KeyCode.UpArrow))
            {
                vertical_direction += 1;
            }

            //S to move down
            if (keys_down.Contains(KeyCode.S) || keys_down.Contains(KeyCode.DownArrow))
            {
                vertical_direction -= 1;
            }
        }

        //update orientation
        if (orientation_direction != 0)
        {
            if (orientation_direction > 0)
            {
                orientation_lever_angle = Mathf.Max(-35.0f, orientation_lever_angle - Time.deltaTime * 100.0f);
            }
            else
            {
                orientation_lever_angle = Mathf.Min(35.0f, orientation_lever_angle + Time.deltaTime * 100.0f);
            }
        }
        else
        {
            if (orientation_lever_angle > 0.0f)
            {
                orientation_lever_angle = Mathf.Max(0.0f, orientation_lever_angle - Time.deltaTime * 100.0f);
            }
            else
            {
                orientation_lever_angle = Mathf.Min(0.0f, orientation_lever_angle + Time.deltaTime * 100.0f);
            }
        }
        //update vertical direction
        if (vertical_direction != 0)
        {
            if (vertical_direction > 0)
            {
                vertical_lever_angle = Mathf.Min(35.0f, vertical_lever_angle + Time.deltaTime * 100.0f);
            }
            else
            {
                vertical_lever_angle = Mathf.Max(-35.0f, vertical_lever_angle - Time.deltaTime * 100.0f);
            }
        }
        else
        {
            if (vertical_lever_angle > 0.0f)
            {
                vertical_lever_angle = Mathf.Max(0.0f, vertical_lever_angle - Time.deltaTime * 100.0f);
            }
            else
            {
                vertical_lever_angle = Mathf.Min(0.0f, vertical_lever_angle + Time.deltaTime * 100.0f);
            }
        }
        //if lever is pushed in either direction at all
        if (Mathf.Abs(orientation_lever_angle) > 0.0f)
        {
            if (orientation_lever_angle > 0.0f)
            {
                orientation_angle -= (orientation_lever_angle / 35.0f) * 20f * Time.deltaTime;
            }
            else
            {
                orientation_angle += (orientation_lever_angle / -35.0f) * 20f * Time.deltaTime;
            }
            orientation_angle = (Mathf.Round(orientation_angle * 10) / 10.0f);
            if (orientation_angle > 359.9f)
            {
                orientation_angle -= 360.0f;
            }
            else if (orientation_angle < 0.0f)
            {
                orientation_angle += 360.0f;
            }
        }
        probe_position = probe.transform.localPosition;
        if (Mathf.Abs(lateral_movement_factors[0] - lateral_movement_factors[2]) > 0.0f)
        {
            probe_position += probe.transform.forward * (lateral_movement_factors[0] - lateral_movement_factors[2]) * Time.deltaTime;
        }
        if (Mathf.Abs(lateral_movement_factors[3] - lateral_movement_factors[1]) > 0.0f)
        {
            probe_position += probe.transform.right * (lateral_movement_factors[3] - lateral_movement_factors[1]) * Time.deltaTime;
        }
        if (Mathf.Abs(vertical_lever_angle) > 0.0f)
        {
            probe_position += probe.transform.up * vertical_lever_angle * Time.deltaTime * 0.1f;
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