/*
    TBPower.cs
    - Handles inputs for tractor beam power
    - Moves tractor beam lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/6/2025
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Windows;

public class TBPower : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "TRACTOR BEAM";
    private List<string> CONTROL_DESCS = new List<string> { "DECREASE", "INCREASE" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };

    public GameObject lever;
    public GameObject display_canvas; //used to display the bars beneath the handle

    private List<KeyCode> keys_down = new List<KeyCode>();

    private float power = 0.0f;
    private int power_direction = 0; //0 is neutral, 1 is increase, and -1 is decrease

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
    private void displayAdjustment()
    {
        //update bars on screen
        int power_as_int = (int)(power * 100.0f);
        if (power_as_int < 100)
        {
            int position = (power_as_int / 5);
            if (display_canvas.transform.childCount > position + 1)
            {
                display_canvas.transform.GetChild(position + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.5f + (0.5f * (position / 20.0f)), 0.5f, 0f, (0.2f * (power_as_int % 5)));
            }
        }

        //update lever position
        lever.transform.localRotation = Quaternion.Euler(-30 + (-80 * power), 0f, 0f);
    }
    void Update()
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
                power = Mathf.Min(1.0f, power + (0.002f * (power / 0.5f) + 0.001f) * Time.deltaTime * 50.0f);
            }
            else
            {
                power = Mathf.Max(0.0f, power - (0.002f * (power / 0.5f) + 0.001f) * Time.deltaTime * 50.0f);
            }
            displayAdjustment();
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs)
    {

        keys_down = inputs;
    }
}
