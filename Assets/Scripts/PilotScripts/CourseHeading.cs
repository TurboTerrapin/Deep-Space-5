/*
    CourseHeading.cs
    - Handles inputs for course heading
    - Moves steering wheel
    - Updates corresponding heading screen
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class CourseHeading : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "COURSE HEADING";
    private List<string> CONTROL_DESCS = new List<string>{"DECREASE", "INCREASE"};
    private List<int> CONTROL_INDEXES = new List<int>(){4, 5};

    public GameObject wheel;
    public GameObject fill_circle;
    public GameObject compass;
    public GameObject heading_text;

    private float heading = 0.0f;
    private float rounded_heading = 0.0f;
    private float wheel_angle = 0.0f; //0.0 is straight, -1.0 is max left, 1.0 is max right
    private int wheel_direction = 0;
    private float momentum = 0.1f; //used to create some acceleration

    private List<KeyCode> keys_down = new List<KeyCode>();
    private float delay_timer = 0.01f;

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
        //rotate compass
        compass.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, heading);

        //set heading text
        string display_heading = rounded_heading.ToString();
        if (!display_heading.Contains("."))
        {
            display_heading += ".0�";
        }
        else
        {
            display_heading += "�";
        }
        heading_text.GetComponent<TMP_Text>().SetText(display_heading);

        //adjust fill circle
        if (wheel_angle >= 0f)
        {
            fill_circle.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            fill_circle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        fill_circle.GetComponent<UnityEngine.UI.Image>().fillAmount = Mathf.Abs(wheel_angle / 2.0f);

        //move physical wheel
        wheel.transform.localRotation = Quaternion.Euler(-112.79f, 180f, 450f * wheel_angle);
    }
    void Update()
    {
        delay_timer -= Time.deltaTime;
        if (delay_timer <= 0.0f)
        {
            int temp_wheel_direction = 0;
            //check inputs
            if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow)) //E to increase
            {
                temp_wheel_direction = 1;       
            }
            if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))  //Q to decrease
            {
                temp_wheel_direction -= 1;
            }
            if (temp_wheel_direction != wheel_direction)
            {
                wheel_direction = temp_wheel_direction;
                momentum = 0.01f;
            }
            else
            {
                if (Mathf.Abs(wheel_angle) < 0.95f)
                {
                    momentum = Mathf.Min(2f, momentum + 0.01f);
                }
                else
                {
                    momentum = Mathf.Min(2f - (1.75f * (Mathf.Abs(wheel_angle))), momentum + 0.01f);
                }
            }
            if (wheel_direction > 0) //increasing heading
            {
                wheel_angle = Mathf.Min(1f, wheel_angle + momentum * 0.001f);
            }
            else if (wheel_direction < 0) //decreasing heading
            {
                wheel_angle = Mathf.Max(-1f, wheel_angle - momentum * 0.001f);
            }
            else //not touching wheel
            {
                if (wheel_angle > 0)
                {
                    wheel_angle = Mathf.Max(0f, wheel_angle - momentum * 0.0002f);
                }
                else
                {
                    wheel_angle = Mathf.Min(0f, wheel_angle + momentum * 0.0002f);
                }
            }

            //update heading
            heading += (wheel_angle * 0.1f);
            if (heading > 360.0f)
            {
                heading -= 360.0f;
            }
            else if (heading < 0.0f)
            {
                heading += 360.0f;
            }
            rounded_heading = (Mathf.Round(heading * 10) / 10.0f);
            
            //display new heading
            displayAdjustment();
            delay_timer = 0.01f;
        }
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        keys_down = inputs;
    }
}
