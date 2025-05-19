/*
    ShipManualSelector.cs
    - Sends inputs to ShipManual (directional, selection, and back)
    Contributor(s): Jake Schott
    Last Updated: 5/18/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipManualSelector : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float PUSH_TIME = 0.25f;
    private static float COOLDOWN_TIME = 0.25f;

    private string CONTROL_NAME = "SHIP MANUAL";
    private List<string> CONTROL_DESCS = new List<string> { "SELECT", "BACK", "UP", "DOWN", "LEFT", "RIGHT" };
    private List<int> CONTROL_INDEXES = new List<int>() { 6, 12, 0, 2, 1, 3 };
    private List<Button> BUTTONS = new List<Button>();

    public List<GameObject> buttons; // 0-3 is up, 4-7 is down

    private Vector3[] initial_pos = new Vector3[2];
    private Vector3 push_direction = new Vector3(0.0022f, -0.0052f, 0f);

    private Coroutine sequence_change_coroutine = null;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[3], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[4], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[5], CONTROL_INDEXES[5], false, true));
        hud_info.setButtons(BUTTONS, 4);

        /*//set initial positions
        for (int i = 0; i < buttons.Count; i++)
        {
            initial_pos[i] = buttons[i].transform.localPosition;
        }*/
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    IEnumerator sequenceAdjustment(int index)
    {
        //set buttons to initial positions
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.localPosition = initial_pos[i];
        }

        Vector3 final_pos = initial_pos[index] + push_direction;

        for (int i = 0; i <= 1; i++)
        {
            float half_time = PUSH_TIME * 0.5f;
            float push_time = half_time;

            while (push_time > 0.0f)
            {
                float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
                push_time = Mathf.Max(0.0f, push_time - dt);

                float push_percentage = 1.0f - (push_time / half_time);
                if (i == 1)
                {
                    push_percentage = (push_time / half_time);
                }

                buttons[index].transform.localPosition =
                    new Vector3(Mathf.Lerp(initial_pos[index].x, final_pos.x, push_percentage),
                                Mathf.Lerp(initial_pos[index].y, final_pos.y, push_percentage),
                                Mathf.Lerp(initial_pos[index].z, final_pos.z, push_percentage));

                yield return null;
            }
        }

        yield return new WaitForSeconds(COOLDOWN_TIME);

        BUTTONS[0].updateInteractable(true);
        BUTTONS[1].updateInteractable(true);

        sequence_change_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        /*if (sequence_change_coroutine == null)
        {
            for (int i = 0; i <= 1; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    BUTTONS[i].toggle(0.1f);
                    if (i == 0)
                    {
                        BUTTONS[1].updateInteractable(false);
                    }
                    else
                    {
                        BUTTONS[0].updateInteractable(false);
                    }
                    transmitDestructSequenceChangeRPC(i);
                    return;
                }
            }
        }*/
    }

    [Rpc(SendTo.Everyone)]
    private void transmitDestructSequenceChangeRPC(int button_index)
    {
        if (sequence_change_coroutine != null)
        {
            StopCoroutine(sequence_change_coroutine);
        }
        sequence_change_coroutine = StartCoroutine(sequenceAdjustment(button_index));
    }
}
