/*
    CourseHeading.cs
    - Handles inputs for steering wheel
    - Moves wheel accordingly
    Contributor(s): Jake Schott, Henryk Musial
    Last Updated: 6/25/2025
*/
//
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CourseHeading : NetworkBehaviour, IControllable
{
    private const float maxAngularVelocity = 1.2f;
    private const float accelerationRate = 1.5f;
    private const float decelerationRate = 4.0f;
    private const float returnSpringForce = 6.0f;
    private const float wheelFriction = 0.95f;

    private string CONTROL_NAME = "COURSE HEADING";
    private List<string> CONTROL_DESCS = new List<string> { "DECREASE", "INCREASE" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject wheel;
    public GameObject fill_circle;
    public GameObject heading_text;
    public GameObject compass;
    private GameObject spaceship;

    // State variables
    private float angularVelocity = 0f;
    public float wheel_angle = 0.0f; // Normalized wheel angle (-1, 1), visual wheel position 
    public float steering_input; // True steering input (Does not register spring oscillations beyond neutral)

    private Coroutine wheel_spin_coroutine = null;
    private List<KeyCode> keys_down = new List<KeyCode>();
    private static HUDInfo hud_info = null;

    private void Start()
    {
        spaceship = GameObject.FindGameObjectWithTag("Spaceship");

        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        hud_info.setButtons(BUTTONS);
    }

    public HUDInfo getHUDinfo(GameObject current_target) => hud_info;

    public float getSteeringValue() => steering_input;

    private void displayAdjustment()
    {
        //adjust blue fill circle beneath steering wheel
        fill_circle.transform.localRotation = Quaternion.Euler(0f, wheel_angle >= 0f ? 180f : 0f, 0f);
        fill_circle.GetComponent<UnityEngine.UI.Image>().fillAmount = Mathf.Abs(wheel_angle / 2.0f);

        //point physical wheel in right direction
        wheel.transform.localRotation = Quaternion.Euler(-113.0f, 0.0f, 450f * wheel_angle);

        //adjust course heading text
        float current_rotation = Mathf.Round(spaceship.transform.rotation.eulerAngles.y * 10.0f) / 10.0f;
        if (current_rotation < 0.0f)
        {
            current_rotation += 360.0f;
        }
        else if (current_rotation >= 360.0f)
        {
            current_rotation -= 360.0f;
        }
        string display_heading = current_rotation.ToString();
        if (display_heading.Contains(".") == false)
        {
            display_heading += ".0";
        }
        heading_text.GetComponent<TMP_Text>().SetText(display_heading + "�");

        //adjust course heading slider
        current_rotation = spaceship.transform.rotation.eulerAngles.y;
        if (current_rotation < 0.0f)
        {
            current_rotation += 360.0f;
        }
        else if (current_rotation >= 360.0f)
        {
            current_rotation -= 360.0f;
        }
        List<GameObject> bars = new List<GameObject>();
        int marker_index = 18 - (int)((current_rotation % 22.5f) / 2.5f);
        int halfway_index = marker_index - 9;
        for (int i = 0; i < 21; i++)
        {
            compass.transform.GetChild(i).gameObject.SetActive(false);
        }
        int[] possible_options = { 315, 270, 225, 180, 135, 90, 45, 0};
        for (int i = 0; i < possible_options.Length; i++)
        {
            if (Mathf.Abs(current_rotation - possible_options[i]) < 22.5f)
            {
                compass.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().SetText(possible_options[i].ToString());
                break;
            }
            if (i == 6)
            {
                compass.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().SetText("0");
            }
        }
        for (int i = 0; i < 19; i++)
        {
            if (i == marker_index)
            {
                if (current_rotation % 45.0f > 22.5f)
                {
                    bars.Add(compass.transform.GetChild(1).gameObject);
                }
                else
                {
                    bars.Add(compass.transform.GetChild(0).gameObject);
                }
            } 
            else if (i == halfway_index)
            {
                if (current_rotation % 45.0f > 22.5f)
                {
                    bars.Add(compass.transform.GetChild(0).gameObject);
                }
                else
                {
                    bars.Add(compass.transform.GetChild(1).gameObject);
                }
            }
            else 
            {
                bars.Add(compass.transform.GetChild(i + 2).gameObject);
            }
        }
        float shift = ((current_rotation % 2.5f) / 2.5f) * -0.01f; //0.01 in distance between markers equals 2.5 degrees
        for (int i = 0; i < 19; i++)
        {
            bars[i].SetActive(true);
            bars[i].transform.localPosition = new Vector3((-0.01f * i) + 0.09f - shift, bars[i].transform.localPosition.y, 0.0f);
        }
    }

    IEnumerator wheelSpinning()
    {
        steering_input = 0f;
        int lastInputDirection = 0;
        bool hasCrossedZeroSinceLastInput = false;

        while (keys_down.Count > 0 || Mathf.Abs(wheel_angle) > 0f || Mathf.Abs(angularVelocity) > 0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            int inputDirection = 0;
            bool isPlayerInputActive = false;

            if (!(ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)))
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)) // E
                {
                    inputDirection = 1;
                    isPlayerInputActive = true;
                }
                else if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down)) // Q
                {
                    inputDirection = -1;
                    isPlayerInputActive = true;
                }

                if (isPlayerInputActive)
                {
                    lastInputDirection = inputDirection;
                    hasCrossedZeroSinceLastInput = false;
                }

                if (inputDirection != 0)
                {
                    if (Mathf.Sign(angularVelocity) != inputDirection && Mathf.Abs(angularVelocity) > 0.1f)
                    {
                        angularVelocity = Mathf.MoveTowards(angularVelocity, 0f, decelerationRate * dt);
                    }
                    else
                    {
                        angularVelocity += inputDirection * accelerationRate * dt;
                        angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularVelocity, maxAngularVelocity);
                    }
                }
                else
                {
                    float springAccel = -wheel_angle * returnSpringForce;
                    angularVelocity += springAccel * dt;
                }
            }
            else
            {
                angularVelocity *= wheelFriction;
            }

            float previousAngle = wheel_angle;
            angularVelocity *= Mathf.Pow(wheelFriction, dt * 60f);
            wheel_angle += angularVelocity * dt;
            wheel_angle = Mathf.Clamp(wheel_angle, -1f, 1f);

            // Detect zero crossing
            if (Mathf.Sign(previousAngle) != Mathf.Sign(wheel_angle) && !isPlayerInputActive)
            {
                hasCrossedZeroSinceLastInput = true;
            }

            if (isPlayerInputActive)
            {
                steering_input = wheel_angle;
            }
            else
            {
                if (hasCrossedZeroSinceLastInput)
                {
                    // Crossed 0 - Ignore all values on the opposite side
                    steering_input = 0f;
                }
                else
                {
                    // Clamp the steering input to avoid registering oscillations past neutral
                    if (lastInputDirection == 1) // last input was right
                    {
                        steering_input = Mathf.Clamp(wheel_angle, 0f, 1f); // Clamp [0, 1]
                    }
                    else if (lastInputDirection == -1) // last input was left
                    {
                        steering_input = Mathf.Clamp(wheel_angle, -1f, 0f); // Clamp [-1, 0]
                    }
                    else 
                    {
                        steering_input = 0f;
                    }
                }
            }

            // Reset the wheel to the neutral position
            if (Mathf.Abs(wheel_angle) < 0.001f && Mathf.Abs(angularVelocity) < 0.01f)
            {
                wheel_angle = Mathf.MoveTowards(wheel_angle, 0.0f, Time.deltaTime * 0.001f);
                angularVelocity = 0f;
                steering_input = 0f;
                hasCrossedZeroSinceLastInput = false;
            }

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
        if (wheel_spin_coroutine == null && inputs.Count > 0)
        {
            wheel_spin_coroutine = StartCoroutine(wheelSpinning());
        }
    }
}