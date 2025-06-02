using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CourseHeading : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float TURN_SPEED = 80.0f;
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
        return wheel_angle; 
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

    public float filtered_wheel_angle;
    IEnumerator wheelSpinning()
    {

        /*
        float angularVelocity = 0f;
        float maxAngularVelocity = 1.5f;
        float accelerationRate = 3.0f; // How fast player can spin the wheel
        float decelerationRate = 6.0f; // How quickly the wheel slows down when opposing input
        float returnSpringForce = 12.0f; // How strongly the wheel returns to center
        float wheelFriction = 0.95f; // General damping for everything
        */

        float angularVelocity = 0f;
        float maxAngularVelocity = 1.2f;
        float accelerationRate = 1.5f; // How fast player can spin the wheel
        float decelerationRate = 4.0f; // How quickly the wheel slows down when opposing input
        float returnSpringForce = 6.0f; // How strongly the wheel returns to center
        float wheelFriction = 0.95f; // General damping for everything


        filtered_wheel_angle = 0f;
        bool hasCrossedZero = false;

    
        while (keys_down.Count > 0 || Mathf.Abs(wheel_angle) > 0.001f || Mathf.Abs(angularVelocity) > 0.001f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            int inputDirection = 0;

            if (!(ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)))
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)) // E
                    inputDirection = 1;
                else if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down)) // Q
                    inputDirection = -1;

                // When input is present
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


            angularVelocity *= Mathf.Pow(wheelFriction, dt * 60f);

            wheel_angle += angularVelocity * dt;
            wheel_angle = Mathf.Clamp(wheel_angle, -1f, 1f);

            // When returning to center, freeze input once it bounces across zero
            if (!hasCrossedZero)
            {
                // While moving toward center
                if (Mathf.Abs(wheel_angle) < 0.05f && Mathf.Abs(angularVelocity) < 0.2f)
                {
                    hasCrossedZero = true;
                    filtered_wheel_angle = 0f;
                }
                else
                {
                    filtered_wheel_angle = wheel_angle;
                }
            }
            else
            {
                // Stay locked at zero until clear player input resumes
                if (Mathf.Abs(wheel_angle) > 0.05f)
                {
                    hasCrossedZero = false;
                    filtered_wheel_angle = wheel_angle;
                }
            }

            if (Mathf.Abs(wheel_angle) < 0.001f && Mathf.Abs(angularVelocity) < 0.01f)
            {
                wheel_angle = 0f;
                angularVelocity = 0f;
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