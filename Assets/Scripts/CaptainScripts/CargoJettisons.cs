/*
    PhaserPowers.cs
    - Determines whether phasers are enabled or not
    Contributor(s): Jake Schott
    Last Updated: 5/14/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CargoJettisons : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float ARM_TIME = 1.5f;
    private static float PUSH_TIME = 1.0f;
    private static float COOLDOWN_TIME = 3.0f;

    private string[] CONTROL_NAMES = new string[4] { "CARGO JETTISON A", "CARGO JETTISON B", "CARGO JETTISON C", "CARGO JETTISON D" };
    private List<string> CONTROL_DESCS = new List<string> { "EJECT", "ARM" };
    private List<int> CONTROL_INDEXES = new List<int>() { 6, 11 };
    private List<Button>[] BUTTON_LISTS = new List<Button>[4] { new List<Button>(), new List<Button>(), new List<Button>(), new List<Button>() };

    public List<GameObject> dials = null;

    private Coroutine[] dial_turn_coroutines = { null, null, null, null};
    private Coroutine[] cargo_eject_coroutines = { null, null, null, null };
    private float[] dial_turn_percentages = { 0.0f, 0.0f, 0.0f, 0.0f };
    private Vector3[] initial_pos = new Vector3[4];
    private Vector3 push_direction = new Vector3(0.006f, -0.0151f, 0.0f);

    private List<KeyCode> keys_down = new List<KeyCode>();
    private List<string> ray_targets = new List<string> { "cargo_jettison_a", "cargo_jettison_b", "cargo_jettison_c", "cargo_jettison_d" };

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);

        for (int i = 0; i < 4; i++)
        {
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, true));
            BUTTON_LISTS[i].Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
            initial_pos[i] = dials[i].transform.localPosition;
        }

        hud_info.setButtons(BUTTON_LISTS[0]);
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        hud_info.setTitle(CONTROL_NAMES[index]);
        hud_info.setButtons(BUTTON_LISTS[index]);
        return hud_info;
    }

    private void displayDialTurn(int index)
    {
        dials[index].transform.localRotation =
            Quaternion.Euler(dials[index].transform.localEulerAngles.x,
                             dials[index].transform.localEulerAngles.y,
                             Mathf.Lerp(-90.0f, -180.0f, dial_turn_percentages[index]));
    }

    IEnumerator dialTurn(int index)
    {
        while ((keys_down.Count > 0 || dial_turn_percentages[index] > 0.0f) && cargo_eject_coroutines[index] == null)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)){
                dial_turn_percentages[index] = Mathf.Min(1.0f, dial_turn_percentages[index] + (dt / ARM_TIME));
            }
            else
            {
                dial_turn_percentages[index] = Mathf.Max(0.0f, dial_turn_percentages[index] - (dt / ARM_TIME));
            }

            BUTTON_LISTS[index][0].updateInteractable(dial_turn_percentages[index] >= 1.0f);

            transmitDialArmRPC(index, dial_turn_percentages[index]);

            keys_down.Clear();
            yield return null;
        }

        dial_turn_coroutines[index] = null;
    }

    IEnumerator ejectCargo(int index)
    {
        dials[index].transform.localPosition = initial_pos[index];
        dials[index].transform.localRotation =
            Quaternion.Euler(dials[index].transform.localEulerAngles.x,
                             dials[index].transform.localEulerAngles.y,
                             -180.0f);

        Vector3 final_pos = initial_pos[index] + push_direction;

        //push the dial in
        float push_time = PUSH_TIME;
        while (push_time > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            push_time = Mathf.Max(0.0f, push_time - dt);

            dials[index].transform.localPosition =
                new Vector3(Mathf.Lerp(initial_pos[index].x, final_pos.x, 1.0f - (push_time / PUSH_TIME)),
                            Mathf.Lerp(initial_pos[index].y, final_pos.y, 1.0f - (push_time / PUSH_TIME)),
                            Mathf.Lerp(initial_pos[index].z, final_pos.z, 1.0f - (push_time / PUSH_TIME)));

            yield return null;
        }

        //bring the dial back and unrotate
        float cooldown_time = COOLDOWN_TIME;
        while (cooldown_time > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            cooldown_time = Mathf.Max(0.0f, cooldown_time - dt);

            dials[index].transform.localPosition =
                new Vector3(Mathf.Lerp(initial_pos[index].x, final_pos.x, cooldown_time / COOLDOWN_TIME),
                            Mathf.Lerp(initial_pos[index].y, final_pos.y, cooldown_time / COOLDOWN_TIME),
                            Mathf.Lerp(initial_pos[index].z, final_pos.z, cooldown_time / COOLDOWN_TIME));

            dials[index].transform.localRotation =
                Quaternion.Euler(dials[index].transform.localEulerAngles.x,
                                 dials[index].transform.localEulerAngles.y,
                                 Mathf.Lerp(-90.0f, -180.0f, cooldown_time / COOLDOWN_TIME));

            yield return null;
        }

        BUTTON_LISTS[index][1].updateInteractable(true);
        dial_turn_percentages[index] = 0.0f;

        cargo_eject_coroutines[index] = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;

        int index = ray_targets.IndexOf(current_target.name);
        if (dial_turn_percentages[index] >= 1.0f && cargo_eject_coroutines[index] == null)
        {
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], inputs))
            {
                BUTTON_LISTS[index][0].toggle(0.2f);
                BUTTON_LISTS[index][1].updateInteractable(false);
                transmitEjectRPC(index);
            }
        }
        if (dial_turn_percentages[index] == 0.0f && cargo_eject_coroutines[index] == null)
        {
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], inputs))
            {
                if (dial_turn_coroutines[index] == null)
                {
                    dial_turn_coroutines[index] = StartCoroutine(dialTurn(index));
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitDialArmRPC(int index, float dp)
    {
        dial_turn_percentages[index] = dp;
        displayDialTurn(index);
    }

    [Rpc(SendTo.Everyone)]
    private void transmitEjectRPC(int index)
    {
        if (dial_turn_coroutines[index] != null)
        {
            StopCoroutine(dial_turn_coroutines[index]);
            dial_turn_coroutines[index] = null;
        }
        if (cargo_eject_coroutines[index] != null)
        {
            StopCoroutine(cargo_eject_coroutines[index]);
        }
        cargo_eject_coroutines[index] = StartCoroutine(ejectCargo(index));
    }
}
