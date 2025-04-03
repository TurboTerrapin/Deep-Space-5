/*
    TractorBeam.cs
    - Handles inputs for tractor beam
    - Moves tractor beam lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/1/2025
*/

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class TractorBeam : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "TRACTOR BEAM";
    private List<string> CONTROL_DESCS = new List<string>{"DECREASE", "INCREASE"};
    private List<int> CONTROL_INDEXES = new List<int>(){4, 5};

    public GameObject lever;
    public GameObject display_canvas; //used to display the bars beneath the handle

    private float power = 0.0f;
    private float temp_power;

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
    private void adjustImpulse() //adjusts physical position of lever and power value after inputs
    {
        if (temp_power != power) //lever was adjusted
        {
            int power_as_int = (int)(power * 100.0f);
            if (power_as_int < 100)
            {
                if (temp_power < power)
                {
                    for (int i = 20; i > (power_as_int / 5) + 1; i--)
                    {
                        if (display_canvas.transform.childCount > i && i != 1)
                        {
                            Destroy(display_canvas.transform.GetChild(i).gameObject);
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= power_as_int / 5; i++)
                    {
                        if (display_canvas.transform.childCount <= i + 1)
                        {
                            GameObject new_bar = UnityEngine.Object.Instantiate(display_canvas.transform.GetChild(1).gameObject, display_canvas.transform);
                            new_bar.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.5f + (0.5f * (i / 20f)), 0.5f, 0f, 1.0f);
                            new_bar.transform.localPosition = new Vector3(0f, -0.1551f + i * 0.016f, 0f);
                            new_bar.name = "Rectangle" + i;
                        }
                    }
                }
                int position = (power_as_int / 5);
                if (display_canvas.transform.childCount > position + 1)
                {
                    display_canvas.transform.GetChild(position + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.5f + (0.5f * (position / 20.0f)), 0.5f, 0f, (0.2f * (power_as_int % 5)));
                }
            }
            power = temp_power;
            lever.transform.localRotation = Quaternion.Euler(-30 + (-80 * power), 0f, 0f);
        }
    }
    private void increment()
    {
        temp_power += 0.002f * (power / 0.5f) + 0.001f;
        temp_power = Mathf.Min(temp_power, 1.0f);
    }
    private void decrement()
    {
        temp_power -= 0.002f * (power / 0.5f) + 0.001f;
        temp_power = Mathf.Max(temp_power, 0.0f);
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        if (inputs.Contains(KeyCode.E) || inputs.Contains(KeyCode.RightArrow)) //E to increment
        {
            increment();
        }
        if (inputs.Contains(KeyCode.Q) || inputs.Contains(KeyCode.LeftArrow))  //Q to decrement
        {
            decrement();
        }
        adjustImpulse();
    }
}
