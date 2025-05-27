/*
    ProbeLateralMovement.cs
    - Pushes in lateral movement buttons
    - Adjusts screen
    - Affects probe
    Contributor(s): Jake Schott
    Last Updated: 5/15/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProbeLateralMovement : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float BUTTON_SPEED = 10.0f;
    private static float PROBE_SPEED = 2.0f;

    private string CONTROL_NAME = "PROBE LATERAL MOVEMENT";
    private List<string> CONTROL_DESCS = new List<string> {"FORWARD", "LEFT", "REVERSE", "RIGHT"};
    private List<int> CONTROL_INDEXES = new List<int>() {0, 1, 2, 3};
    private List<Button> BUTTONS = new List<Button>();

    public List<GameObject> lateral_buttons = null; //forward, left, reverse, right
    public GameObject lateral_canvas;
    public GameObject probe;

    private Vector3[] initial_positions = new Vector3[4];
    private Vector3[] final_positions = new Vector3[4];
    private float[] lateral_movement_factors = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f }; //forward, left, reverse, right
    private Vector3 lateral_button_move_direction = new Vector3(0, -0.006f, 0.0024f);
    private Vector3 probe_position;
    private Coroutine lateral_adjustment_coroutine = null;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[3], true, false));
        hud_info.setButtons(BUTTONS);

        for (int i = 0; i <= 3; i++)
        {
            initial_positions[i] = lateral_buttons[i].transform.localPosition;
            final_positions[i] = lateral_buttons[i].transform.localPosition + lateral_button_move_direction;
        }

        probe_position = probe.transform.localPosition;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }
    private void displayAdjustment()
    {
        //push lateral buttons, update circle
        for (int i = 0; i <= 3; i++)
        {
            lateral_buttons[i].transform.localPosition =
                new Vector3(Mathf.Lerp(initial_positions[i].x, final_positions[i].x, lateral_movement_factors[i]),
                            Mathf.Lerp(initial_positions[i].y, final_positions[i].y, lateral_movement_factors[i]),
                            Mathf.Lerp(initial_positions[i].z, final_positions[i].z, lateral_movement_factors[i]));

            lateral_canvas.transform.GetChild(i + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.83f, 1f, lateral_movement_factors[i]);
        }

        //update probe
        probe.transform.localPosition = probe_position;
    }

    private bool isNeutralState()
    {
        for (int i = 0; i <= 3; i++)
        {
            if (lateral_movement_factors[i] != 0.0f)
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator lateralAdjustment()
    {
        while (keys_down.Count > 0 || !isNeutralState())
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);

            probe_position = probe.transform.localPosition;

            for (int i = 0; i <= 3; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], keys_down))
                {
                    lateral_movement_factors[i] = Mathf.Min(1.0f, lateral_movement_factors[i] + dt * BUTTON_SPEED);
                }
                else
                {
                    lateral_movement_factors[i] = Mathf.Max(0.0f, lateral_movement_factors[i] - dt * BUTTON_SPEED);
                }
            }

            if (Mathf.Abs(lateral_movement_factors[0] - lateral_movement_factors[2]) > 0.0f)
            {
                probe_position += probe.transform.forward * (lateral_movement_factors[0] - lateral_movement_factors[2]) * dt * PROBE_SPEED;
            }
            if (Mathf.Abs(lateral_movement_factors[3] - lateral_movement_factors[1]) > 0.0f)
            {
                probe_position += probe.transform.right * (lateral_movement_factors[3] - lateral_movement_factors[1]) * dt * PROBE_SPEED;
            }

            for (int i = 0; i <= 3; i++)
            {
                if (lateral_movement_factors[i] != 1.0f)
                {
                    transmitProbeLateralAdjustmentRPC(probe_position, lateral_movement_factors[0], lateral_movement_factors[1], lateral_movement_factors[2], lateral_movement_factors[3]);
                    break;
                }
            }

            keys_down.Clear();
            yield return null;
        }

        lateral_adjustment_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (lateral_adjustment_coroutine == null)
        {
            for (int i = 0; i < CONTROL_INDEXES.Count; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    lateral_adjustment_coroutine = StartCoroutine(lateralAdjustment());
                    return;
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitProbeLateralAdjustmentRPC(Vector3 new_pos, float fwd, float left, float rev, float right)
    {
        lateral_movement_factors[0] = fwd;
        lateral_movement_factors[1] = left;
        lateral_movement_factors[2] = rev;
        lateral_movement_factors[3] = right;
        probe_position = new_pos;
        displayAdjustment();
    }
}
