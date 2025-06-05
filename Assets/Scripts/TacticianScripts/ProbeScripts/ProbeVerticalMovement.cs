/*
    ProbeVerticalMovement.cs
    - Turns lever
    - Adjusts screen
    - Affects probe
    Contributor(s): Jake Schott
    Last Updated: 5/15/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProbeVerticalMovement : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float LEVER_SPEED = 100.0f;
    private static float PROBE_SPEED = 0.1f;

    private string CONTROL_NAME = "PROBE VERTICAL MOVEMENT";
    private List<string> CONTROL_DESCS = new List<string> {"DESCEND", "ASCEND"};
    private List<int> CONTROL_INDEXES = new List<int>() {2,0};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject vertical_lever;
    public GameObject vertical_canvas;
    public GameObject probe;

    private float vertical_lever_angle = 0.0f;
    private Vector3 probe_position;
    private Coroutine vertical_adjustment_coroutine = null;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        hud_info.setButtons(BUTTONS);

        probe_position = probe.transform.localPosition;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }
    private void displayAdjustment()
    {
        //update lever position
        vertical_lever.transform.localRotation = Quaternion.Euler(-70f - vertical_lever_angle, 180f, -90f);

        //update probe
        probe.transform.localPosition = probe_position;

        //lastly, update altitude screen
        vertical_canvas.transform.GetChild(1).transform.localPosition = new Vector3(-0.0095f, 0.045f * (probe.transform.localPosition.y / 50f), 0);
    }

    private bool isNeutralState()
    {
        return (vertical_lever_angle == 0.0f);
    }

    IEnumerator verticalAdjustment()
    {
        while (keys_down.Count > 0 || !isNeutralState())
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);

            probe_position = probe.transform.localPosition;

            int vertical_direction = 0;

            if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down))
            {
                vertical_direction += 1;
            }
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down))
            {
                vertical_direction -= 1;
            }

            if (vertical_direction != 0)
            {
                if (vertical_direction > 0)
                {
                    vertical_lever_angle = Mathf.Min(35.0f, vertical_lever_angle + dt * LEVER_SPEED);
                }
                else
                {
                    vertical_lever_angle = Mathf.Max(-35.0f, vertical_lever_angle - dt * LEVER_SPEED);
                }
            }
            else
            {
                if (vertical_lever_angle > 0.0f)
                {
                    vertical_lever_angle = Mathf.Max(0.0f, vertical_lever_angle - dt * LEVER_SPEED);
                }
                else
                {
                    vertical_lever_angle = Mathf.Min(0.0f, vertical_lever_angle + dt * LEVER_SPEED);
                }
            }

            if (Mathf.Abs(vertical_lever_angle) > 0.0f)
            {
                probe_position += probe.transform.up * vertical_lever_angle * dt * PROBE_SPEED;
            }

            if (vertical_lever_angle != 0.0f)
            {
                transmitProbeVerticalAdjustmentRPC(probe_position, vertical_lever_angle);
            }

            keys_down.Clear();
            yield return null;
        }

        vertical_adjustment_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (vertical_adjustment_coroutine == null)
        {
            for (int i = 0; i < CONTROL_INDEXES.Count; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    vertical_adjustment_coroutine = StartCoroutine(verticalAdjustment());
                    return;
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitProbeVerticalAdjustmentRPC(Vector3 new_pos, float ang)
    {
        vertical_lever_angle = ang;
        probe_position = new_pos;
        displayAdjustment();
    }
}