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
using UnityEngine;

public class CourseHeading : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "COURSE HEADING";
    private List<string> CONTROL_DESCS = new List<string>{"DECREASE", "INCREASE"};
    private List<int> CONTROL_INDEXES = new List<int>(){4, 5};

    public GameObject wheel;
    public GameObject compass;
    public GameObject heading_text;

    private float heading = 0.0f;
    private float angle = 0.0f;
    private float temp_angle = 0.0f;
    private float timer = 0.1f;

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
    void FixedUpdate()
    {
        if (Mathf.Abs(angle) > 2.0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                if (heading > 360.0f)
                {
                    heading -= 360.0f;
                }
                else if (heading < 0.0f)
                {
                    heading += 360.0f;
                }
                heading += (angle * 0.01f);
                heading = (Mathf.Round(heading * 10) / 10.0f);
                compass.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, heading);
                string display_heading = heading.ToString();
                if (!display_heading.Contains(".")) 
                {
                    display_heading += ".0°";
                }
                else
                {
                    display_heading += "°";
                }
                    heading_text.GetComponent<TMP_Text>().SetText(display_heading);
                timer = 0.1f;
            }
        }
    }
    private void increment()
    {
        temp_angle += (0.02f + (0.3f * (Mathf.Abs(angle / 90.0f))));
        temp_angle = Mathf.Min(temp_angle, 90.0f);
    }
    private void decrement()
    {
        temp_angle -= (0.02f + (0.3f * (Mathf.Abs(angle / 90.0f))));
        temp_angle = Mathf.Max(temp_angle, -90.0f);
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
        if (temp_angle != angle)
        {
            wheel.transform.Rotate(0.0f,  0.0f, temp_angle - angle, Space.Self);
            angle = temp_angle;
        }
    }
}
