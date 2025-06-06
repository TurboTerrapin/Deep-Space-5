/*
    CourseHeading.cs
    - Handles inputs for course heading
    - Moves steering wheel
    - Updates corresponding heading screen
    Contributor(s): Jake Schott
    Last Updated: 5/12/2025
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class CourseHeading : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float TURN_SPEED = 50.0f;

    private string CONTROL_NAME = "COURSE HEADING";
    private List<string> CONTROL_DESCS = new List<string>{"DECREASE", "INCREASE"};
    private List<int> CONTROL_INDEXES = new List<int>(){4, 5};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject wheel;
    public GameObject fill_circle;
    public GameObject compass;
    public GameObject heading_text;
    public GameObject degrees_symbol;

    private float heading = 0.0f;
    private float rounded_heading = 0.0f;
    private float wheel_angle = 0.0f; //0.0 is straight, -1.0 is max left, 1.0 is max right
    private int wheel_direction = 0;
    private float momentum = 0.1f; //used to create some acceleration
    private Coroutine wheel_spin_coroutine = null;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        hud_info.setButtons(BUTTONS);
    }
    public HUDInfo getHUDinfo(GameObject current_target)
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
            display_heading += ".0";
        }
        heading_text.GetComponent<TMP_Text>().SetText(display_heading);
        degrees_symbol.transform.localPosition = new Vector3(-0.019f + (display_heading.Length - 3) * -0.006f, -0.001f, 0f);

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

    IEnumerator wheelSpinning()
    {
        //only run coroutine when there are keys inputted OR the wheel needs to return to center
        while (keys_down.Count > 0 || (Mathf.Abs(wheel_angle) > 0.0f))
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);

            if (!(ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down))){
                int temp_wheel_direction = 0;
                //check inputs
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)) //E to increase
                {
                    temp_wheel_direction = 1;
                }
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down))  //Q to decrease
                {
                    temp_wheel_direction = -1;
                }

                //check for change in wheel direction
                if (temp_wheel_direction != wheel_direction)
                {
                    wheel_direction = temp_wheel_direction;
                    momentum = 0.01f;
                }
                else
                {
                    //if same direction, then increase momentum
                    if (Mathf.Abs(wheel_angle) < 0.95f)
                    {
                        momentum = Mathf.Min(2f, momentum + (0.01f * dt * TURN_SPEED));
                    }
                    else
                    {
                        momentum = Mathf.Min(2f - (1.75f * (Mathf.Abs(wheel_angle))), momentum + (0.01f * dt * TURN_SPEED));
                    }
                }
                if (wheel_direction > 0) //increasing heading
                {
                    wheel_angle = Mathf.Min(1f, wheel_angle + (momentum * 0.001f * dt * TURN_SPEED));
                }
                else if (wheel_direction < 0) //decreasing heading
                {
                    wheel_angle = Mathf.Max(-1f, wheel_angle - (momentum * 0.001f * dt * TURN_SPEED));
                }
                else //not touching wheel
                {
                    if (wheel_angle > 0)
                    {
                        wheel_angle = Mathf.Max(0f, wheel_angle - (momentum * 0.0002f * dt * TURN_SPEED));
                    }
                    else
                    {
                        wheel_angle = Mathf.Min(0f, wheel_angle + (momentum * 0.0002f * dt * TURN_SPEED));
                    }
                }
            }
            else
            {
                //if both increase and decrease are pressed then do nothing except reset momentum
                momentum = 0.01f;
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
            transmitCourseHeadingRPC(heading, rounded_heading, wheel_angle);
            keys_down.Clear();
            yield return null;
        }

        wheel_spin_coroutine = null;
    }

    [Rpc(SendTo.Everyone)]
    private void transmitCourseHeadingRPC(float head, float round_head, float wheel_ang)
    {
        heading = head;
        rounded_heading = round_head;
        wheel_angle = wheel_ang;
        displayAdjustment();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (wheel_spin_coroutine == null)
        {
            for (int i = 0; i < CONTROL_INDEXES.Count; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs)) 
                {
                    wheel_spin_coroutine = StartCoroutine(wheelSpinning());
                    return;
                }
            }
        }
    }
}