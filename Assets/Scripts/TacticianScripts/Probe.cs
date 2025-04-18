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
    private string[] CONTROL_NAMES = new string[] {"PROBE ORIENTATION"};
    private List<string> CONTROL_DESCS = new List<string> {"TURN LEFT", "TURN RIGHT"};
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button>[] BUTTON_LISTS = new List<Button>[1] {new List<Button>()};

    public GameObject probe;

    public GameObject orientation_lever;
    public GameObject orientation_canvas; //used to display the bars beneath the handle
    private float orientation_lever_angle = 0.0f;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private float orientation_angle = 0.0f;
    private int orientation_direction = 0; //0 is neutral, 1 is increase, and -1 is decrease

    private List<string> ray_targets = new List<string> {"probe_orientation"};
    private int target_index = -1;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));

        hud_info.setButtons(BUTTON_LISTS[0]);
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

        //update lever position
        orientation_lever.transform.localRotation = Quaternion.Euler(-20f, -90f, orientation_lever_angle);

        //update probe
        probe.transform.localRotation = Quaternion.Euler(0f, orientation_angle - 180f, 0f);
    }
    void Update()
    {
        orientation_direction = 0;
        if (target_index == 0)
        {

            if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow)) //E to go right
            {
                orientation_direction += 1;
            }
            if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))  //Q to right
            {
                orientation_direction -= 1;
            }
        }
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
        if (Mathf.Abs(orientation_lever_angle) == 35.0f)
        {
            if (orientation_lever_angle > 0.0f)
            {
                orientation_angle -= 20f * Time.deltaTime;
            }
            else
            {
                orientation_angle += 20f * Time.deltaTime;
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
        displayAdjustment();
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        target_index = ray_targets.IndexOf(current_target.name);
        keys_down = inputs;
    }
}