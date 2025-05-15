/*
    Headlights.cs
    - Handles inputs for headlights
    - Moves physical slider
    - Updates corresponding screen
    Contributor(s): Jake Schott
    Last Updated: 4/13/2025
*/


using System.Collections.Generic;
using UnityEngine;

public class Headlights : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "HEADLIGHTS";
    private List<string> CONTROL_DESCS = new List<string> {"DIM", "BRIGHTEN"};
    private List<int> CONTROL_INDEXES = new List<int>() {2, 0};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject slider;
    public GameObject screen;

    private int slider_configuration = 0;
    private int iterations = 0;
    private float move_factor = -1.0f;
    private float cooldown_timer = 0.02f;
    private float end_pos_y = 0.03994f;
    private float end_pos_z = -18.0192f;
    private float init_pos_y;
    private float init_pos_z;
    private bool adjusting_slider = false;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        hud_info.setButtons(BUTTONS);

        init_pos_y = slider.transform.position.y;
        init_pos_z = slider.transform.position.z;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }
    void LateUpdate()
    {
        if (adjusting_slider == true)
        {
            cooldown_timer -= Time.deltaTime;
            if (cooldown_timer <= 0)
            {
                slider.transform.position = new Vector3(0, slider.transform.position.y + (((end_pos_y - init_pos_y) / 70) * move_factor), slider.transform.position.z + (((end_pos_z - init_pos_z) / 70) * (move_factor * 1)));
                if (move_factor > 0)
                {
                    screen.transform.GetChild(slider_configuration + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.93f, 1.0f, ((10 - iterations) / 10.0f) * (((float)slider_configuration) / 7));
                }
                else
                {
                    screen.transform.GetChild(slider_configuration + 2).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.93f, 1.0f, ((iterations - 1) / 10.0f) * (((float)slider_configuration) / 7));
                }
                iterations--;
                if (iterations <= 0)
                {
                    float dest_pos_y = init_pos_y + (((float)slider_configuration) / 7) * (end_pos_y - init_pos_y);
                    float dest_pos_z = init_pos_z + (((float)slider_configuration) / 7) * (end_pos_z - init_pos_z);
                    slider.transform.position = new Vector3(0, dest_pos_y, dest_pos_z);
                    adjusting_slider = false;
                    if (slider_configuration <= 0)
                    {
                        BUTTONS[0].updateInteractable(false);
                    }
                    else
                    {
                        BUTTONS[0].updateInteractable(true);
                    }
                    if (slider_configuration >= 7)
                    {
                        BUTTONS[1].updateInteractable(false);
                    }
                    else
                    {
                        BUTTONS[1].updateInteractable(true);
                    }
                }
                else
                {
                    cooldown_timer = 0.01f;
                }
            }
        }
    }
    private void increment()
    {
        if (slider_configuration < 7)
        {
            cooldown_timer = 0.1f;
            move_factor = 1.0f;
            iterations = 10;
            slider_configuration++;
            adjusting_slider = true;
        }
    }
    private void decrement()
    {
        if (slider_configuration > 0)
        {
            cooldown_timer = 0.1f;
            move_factor = -1.0f;
            iterations = 10;
            slider_configuration--;
            adjusting_slider = true;
        }
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        if (!((inputs.Contains(KeyCode.W) || inputs.Contains(KeyCode.UpArrow)) && (inputs.Contains(KeyCode.S) || inputs.Contains(KeyCode.DownArrow)))){
            if (adjusting_slider == false)
            {
                if (inputs.Contains(KeyCode.W) || inputs.Contains(KeyCode.UpArrow)) //W to increment
                {
                    increment();
                }
            }
            if (adjusting_slider == false)
            {
                if (inputs.Contains(KeyCode.S) || inputs.Contains(KeyCode.DownArrow))  //S to decrement
                {
                    decrement();
                }
            }
        }
    }
}
