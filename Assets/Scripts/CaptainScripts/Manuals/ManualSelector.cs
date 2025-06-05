/*
    ShipManualSelector.cs
    - Sends inputs to ShipManual and CommunicationsManual (directional, selection, and back)
    Contributor(s): Jake Schott
    Last Updated: 5/22/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManualSelector : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float PUSH_TIME = 0.2f;
    private static float COOLDOWN_TIME = 0.0f;

    private string[] CONTROL_NAMES = new string[] { "SHIP MANUAL", "COMMUNICATIONS MANUAL" };
    private List<string> CONTROL_DESCS = new List<string> { "SELECT", "BACK", "UP", "DOWN", "LEFT", "RIGHT" };
    private List<int> CONTROL_INDEXES = new List<int>() { 6, 12, 0, 2, 1, 3 };
    private List<Button>[] BUTTON_LISTS = new List<Button>[2] { new List<Button>(), new List<Button>() };

    public List<GameObject> buttons;
    public Component[] manuals = new Component[2];

    private Vector3[] initial_pos = new Vector3[12];
    private Vector3[] push_direction = { new Vector3(-0.003f, -0.0074f, 0f), new Vector3(0.003f, -0.0074f, 0f) };

    private bool[] is_active = new bool[] { false, false };
    private Coroutine[] manual_input_coroutine = new Coroutine[] { null, null };

    private List<string> ray_targets = new List<string> { "ship_manual_options", "communications_manual_options" };

    private static HUDInfo hud_info = null;
    private void Start()
    {
        manuals[0] = transform.GetComponent<ShipManual>();
        manuals[1] = transform.GetComponent<CommunicationsManual>();

        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        for (int i = 0; i < 2; i++)
        {
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, true));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], false, true));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], false, true));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[3], CONTROL_INDEXES[3], false, true));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[4], CONTROL_INDEXES[4], false, true));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[5], CONTROL_INDEXES[5], false, true));
        }

        hud_info.setButtons(BUTTON_LISTS[0], 4);

        //set initial positions
        for (int i = 0; i < buttons.Count; i++)
        {
            initial_pos[i] = buttons[i].transform.localPosition;
        }
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        hud_info.setTitle(CONTROL_NAMES[index]);
        hud_info.setButtons(BUTTON_LISTS[index], 4);

        return hud_info;
    }

    public void activate(int index)
    {
        is_active[index] = true;
        updateButtons(index);
    }

    public void deactivate(int index)
    {
        is_active[index] = false;
        for (int i = 0; i <= 5; i++)
        {
            BUTTON_LISTS[index][i].updateInteractable(false);
        }
    }

    private void updateButtons(int index)
    {
        Manual curr_manual = (Manual)manuals[index];
        bool[] curr_options = curr_manual.getInteractableOptions();
        for (int i = 0; i <= 5; i++)
        {
            BUTTON_LISTS[index][i].untoggle();
            BUTTON_LISTS[index][i].updateInteractable(curr_options[i]);
        }
    }

    IEnumerator manualInput(int button, int manual_index)
    {
        //set buttons to initial positions
        int start_i = manual_index * 6;
        for (int i = start_i; i < start_i + 6; i++)
        {
            buttons[i].transform.localPosition = initial_pos[i];
        }

        Vector3 final_pos = initial_pos[button + start_i] + push_direction[manual_index];

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

                buttons[button + start_i].transform.localPosition =
                    new Vector3(Mathf.Lerp(initial_pos[button + start_i].x, final_pos.x, push_percentage),
                                Mathf.Lerp(initial_pos[button + start_i].y, final_pos.y, push_percentage),
                                Mathf.Lerp(initial_pos[button + start_i].z, final_pos.z, push_percentage));

                yield return null;
            }

            Manual curr_manual = (Manual)manuals[manual_index];
            if (i == 0)
            {
                if (button == 0)
                {
                    curr_manual.forward();
                }
                else if (button == 1)
                {
                    curr_manual.back();
                }
                else
                {
                    curr_manual.switchButtons(button - 2);
                }
            }
        }

        yield return new WaitForSeconds(COOLDOWN_TIME);

        updateButtons(manual_index);

        manual_input_coroutine[manual_index] = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        int manual_index = ray_targets.IndexOf(current_target.name);
        Manual curr_manual = (Manual)manuals[manual_index];
        if (manual_input_coroutine[manual_index] == null && is_active[manual_index] == true)
        {
            for (int i = 0; i <= 5; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs) && curr_manual.isValidInput(i) == true)
                {
                    BUTTON_LISTS[manual_index][i].toggle();
                    for (int x = 0; x <= 5; x++)
                    {
                        if (i != x)
                        {
                            BUTTON_LISTS[manual_index][x].updateInteractable(false);
                        }
                    }
                    transmitManualInputRPC(i, manual_index);
                    return;
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitManualInputRPC(int button_index, int manual_index)
    {
        if (manual_input_coroutine[manual_index] != null)
        {
            StopCoroutine(manual_input_coroutine[manual_index]);
        }
        manual_input_coroutine[manual_index] = StartCoroutine(manualInput(button_index, manual_index));
    }
}
