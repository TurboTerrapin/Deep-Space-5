/*
    CourseHeading.cs
    - Handles inputs for course heading
    - Moves steering wheel
    - Returns interpolated steering value (-1 to 1)
    - Modified for fast return-to-center behavior
    Contributor(s): Jake Schott
    Last Updated: 6/2/2025
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CourseHeading : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float TURN_SPEED = 60.0f;
    private static float RETURN_SPEED = 80.0f; 

    private string CONTROL_NAME = "COURSE HEADING";
    private List<string> CONTROL_DESCS = new List<string> { "DECREASE", "INCREASE" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject wheel;
    public GameObject fill_circle;
    public GameObject compass;
    public GameObject heading_text;
    public GameObject degrees_symbol;

    private float wheel_angle = 0.0f; //0.0 is straight, -1.0 is max left, 1.0 is max right
    private int wheel_direction = 0;
    private float momentum = 0.1f; 
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

    public float getSteeringValue()
    {
        return wheel_angle; // Directly return the wheel angle which is already between -1 and 1
    }

    private void displayAdjustment()
    {
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
        wheel.transform.localRotation = Quaternion.Euler(-113.0f, 0.0f, 450f * wheel_angle);
    }

    IEnumerator wheelSpinning()
    {
        //only run coroutine when there are keys inputted OR the wheel needs to return to center
        while (keys_down.Count > 0 || (Mathf.Abs(wheel_angle) > 0.0f))
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);

            if (!(ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)))
            {
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
                else //not touching wheel - FAST RETURN TO CENTER
                {
                    // Increased return speed by multiplying with RETURN_SPEED constant
                    if (wheel_angle > 0)
                    {
                        wheel_angle = Mathf.Max(0f, wheel_angle - (momentum * 0.0002f * dt * TURN_SPEED * RETURN_SPEED));
                    }
                    else
                    {
                        wheel_angle = Mathf.Min(0f, wheel_angle + (momentum * 0.0002f * dt * TURN_SPEED * RETURN_SPEED));
                    }
                }
            }
            else
            {
                //if both increase and decrease are pressed then do nothing except reset momentum
                momentum = 0.01f;
            }

            //display new wheel position
            transmitWheelAngleRPC(wheel_angle);
            keys_down.Clear();
            yield return null;
        }

        wheel_spin_coroutine = null;
    }

    [Rpc(SendTo.Everyone)]
    private void transmitWheelAngleRPC(float wheel_ang)
    {
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