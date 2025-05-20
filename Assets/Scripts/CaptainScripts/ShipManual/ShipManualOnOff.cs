/*
    DestructSequence.cs
    - Used to input the four-digit self-destruct code
    Contributor(s): Jake Schott
    Last Updated: 5/17/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Build;


public class ShipManualOnOff : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float SWITCH_TIME = 0.5f;

    private string CONTROL_NAME = "SHIP MANUAL";
    private List<string> CONTROL_DESCS = new List<string> { "TURN ON", "TURN OFF" };
    private List<int> CONTROL_INDEXES = new List<int>() { 6 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject power_switch;

    private Coroutine power_change_coroutine = null;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        hud_info.setButtons(BUTTONS);
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }
    public void reactivate()
    {
        bool ce = transform.GetComponent<ShipManual>().getCurrentlyEnabled();
        if (ce == true)
        {
            BUTTONS[0].updateDesc(CONTROL_DESCS[1]);
        }
        else
        {
            BUTTONS[0].updateDesc(CONTROL_DESCS[0]);
        }

        BUTTONS[0].updateInteractable(true);
    }

    IEnumerator powerChangeAdjustment(bool to_switch_to, int msg)
    {
        float switch_time = SWITCH_TIME;

        //flip switch, fill meter
        while (switch_time > 0)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            switch_time = Mathf.Max(0.0f, switch_time - dt);

            float lever_angle = Mathf.Lerp(-50f, -100f, switch_time / SWITCH_TIME);

            if (to_switch_to == true)
            {
                lever_angle = Mathf.Lerp(-50f, -100f, 1.0f - (switch_time / SWITCH_TIME));
            }

            power_switch.transform.localRotation =
                Quaternion.Euler(lever_angle, -90f, 90f);

            yield return null;
        }

        transform.GetComponent<ShipManual>().powerSwitch(to_switch_to, msg);

        power_change_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        if (power_change_coroutine == null)
        {
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], inputs) && transform.GetComponent<ShipManual>().getCurrentlyAnimating() == false)
            {
                BUTTONS[0].toggle(0.2f);
                int welcome_message = transform.GetComponent<ShipManual>().pickWelcomeMessage();
                bool currently_enabled = transform.GetComponent<ShipManual>().getCurrentlyEnabled();
                transmitShipManualPowerChangeRPC(!currently_enabled, welcome_message);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitShipManualPowerChangeRPC(bool to_switch_to, int msg)
    {
        if (power_change_coroutine != null)
        {
            StopCoroutine(power_change_coroutine);
        }
        power_change_coroutine = StartCoroutine(powerChangeAdjustment(to_switch_to, msg));
    }
}
