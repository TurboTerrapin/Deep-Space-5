/*
    ThrusterControl.cs
    - Defines binary left/right/up/down structure for thrusters
    - Handles physical buttons
    - Meant to be extended
    Contributor(s): Jake Schott
    Last Updated: 10/5/2026
*/

using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ThrusterControl : NetworkBehaviour
{
    //CLASS CONSTANTS
    protected static float PUSH_SPEED = 20.0f; //how fast the physical button takes to be pushed relative to the bars
    protected static float MOVE_SPEED = 0.1f;
    protected static float MAX_POWER_CONSUMPTION = 0.1f; //equates to 1 circle
    protected static Vector3 BUTTON_FINAL_POS = new Vector3(0.0f, -0.006f, 0.0025f);

    public List<Transform> thruster_buttons;
    public List<AudioSource> thruster_sounds;
    public GameObject thruster_display;

    protected float[] thruster_percentage = new float[2] { 0.0f, 0.0f };
    protected float[] button_push_percentage = new float[2] { 0.0f, 0.0f };
    protected float inertial_dampener_modifier = 0.0f;
    protected float thrust_direction = 0;
    protected Coroutine thruster_coroutine;

    protected HUDInfo hud_info = null;

    public void adjustInertialDampenerModifier(float new_modifier)
    {
        inertial_dampener_modifier = new_modifier;
    }

    protected void updateThrust()
    {
        thrust_direction = thruster_percentage[1] - thruster_percentage[0];
        float greatest_thruster = Mathf.Max(thruster_percentage[0], thruster_percentage[1]);
        ReferenceAssistor.Instance.power_manager.controlPowerChange(0, this.GetType().Name, greatest_thruster * MAX_POWER_CONSUMPTION);
        hud_info.setPowerConsumption(greatest_thruster * MAX_POWER_CONSUMPTION);
    }

    protected bool checkNeutralState()
    {
        for (int i = 0; i < 2; i++)
        {
            if (thruster_percentage[i] > 0.0f || button_push_percentage[i] > 0.0f)
            {
                return false;
            }
        }
        return true;
    }

    protected void adjustThrusterSounds()
    {
        for (int i = 0; i < 2; i++)
        {
            thruster_sounds[i].volume = thruster_percentage[i] * 0.5f;
        }
    }

    protected void adjustButton(Transform thruster_button, int button_index)
    {
        //push the physical button
        thruster_button.transform.localPosition = Vector3.Lerp(Vector3.zero, BUTTON_FINAL_POS, button_push_percentage[button_index]);

        //handle thruster bars
        for (int i = 0; i < 10; i++)
        {
            thruster_display.transform.GetChild(button_index + 1).GetChild(i).gameObject.SetActive(thruster_percentage[button_index] > (i * 0.1f));
        }
    }
}