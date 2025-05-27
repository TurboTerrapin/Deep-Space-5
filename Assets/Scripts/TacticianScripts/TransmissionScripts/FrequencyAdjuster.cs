/*
    FrequencyAdjuster.cs
    - Switches frequencies
    Contributor(s): Jake Schott
    Last Updated: 5/17/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrequencyAdjuster : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float PUSH_TIME = 0.25f;
    private static float SLOW_BUTTON_SPEED = 3.0f;

    private string CONTROL_NAME = "FREQUENCY ADJUSTER";
    private List<string> CONTROL_DESCS = new List<string>{ "SLOW", "PREVIOUS", "NEXT" };
    private List<int> CONTROL_INDEXES = new List<int>(){6, 4, 5};
    private List<Button> BUTTONS = new List<Button>();

    public List<GameObject> frequency_buttons;
    public GameObject slow_button;

    private Vector3[] freq_initial_pos = new Vector3[2];
    private Vector3 freq_push_direction = new Vector3(0.0013f, -0.0036f, 0f);
    private Vector3 sb_initial_pos = new Vector3();
    private Vector3 sb_final_pos = new Vector3(-3.18562f, 8.7358f, 3.7713f);
    private float slow_button_percentage = 0.0f;
    private bool slow_button_active = false;

    private Coroutine frequency_adjustment_coroutine = null;
    private Coroutine slow_button_coroutine = null;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], true, true));
        hud_info.setButtons(BUTTONS, 5);

        //set initial positions
        for (int i = 0; i < 2; i++)
        {
            freq_initial_pos[i] = frequency_buttons[i].transform.localPosition;
        }
        sb_initial_pos = slow_button.transform.localPosition;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    IEnumerator adjustFrequency(int index)
    {
        //set buttons to initial positions
        for (int i = 0; i <= 1; i++)
        {
            frequency_buttons[i].transform.localPosition = freq_initial_pos[i];
        }

        Vector3 final_pos = freq_initial_pos[index] + freq_push_direction;

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

                frequency_buttons[index].transform.localPosition =
                    new Vector3(Mathf.Lerp(freq_initial_pos[index].x, final_pos.x, push_percentage),
                                Mathf.Lerp(freq_initial_pos[index].y, final_pos.y, push_percentage),
                                Mathf.Lerp(freq_initial_pos[index].z, final_pos.z, push_percentage));

                yield return null;
            }

            if (i == 0)
            {
                transform.gameObject.GetComponent<TransmissionHandler>().updateDisplay();
            }
        }

        BUTTONS[1].updateInteractable(true);
        BUTTONS[2].updateInteractable(true);

        frequency_adjustment_coroutine = null;
    }

    private void displaySlowButtonPositionAdjustment()
    {
        slow_button.transform.localPosition =
            new Vector3(Mathf.Lerp(sb_initial_pos.x, sb_final_pos.x, slow_button_percentage),
                        Mathf.Lerp(sb_initial_pos.y, sb_final_pos.y, slow_button_percentage),
                        Mathf.Lerp(sb_initial_pos.z, sb_final_pos.z, slow_button_percentage));
    }

    IEnumerator slowButton()
    {
        while (keys_down.Count > 0 || slow_button_percentage > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            
            bool prev_slow_button_active = slow_button_active;
            float prev_slow_button_percentage = slow_button_percentage;

            slow_button_active = ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down);

            if (slow_button_active == true)
            {
                slow_button_percentage = Mathf.Min(1.0f, slow_button_percentage + dt * SLOW_BUTTON_SPEED);
            }
            else
            {
                slow_button_percentage = Mathf.Max(0.0f, slow_button_percentage - dt * SLOW_BUTTON_SPEED);
            }

            if (slow_button_active != prev_slow_button_active)
            {
                transmitSlowButtonChangeRPC(slow_button_active);
            }

            if (slow_button_percentage != prev_slow_button_percentage)
            {
                transmitSlowButtonPositionRPC(slow_button_percentage);
            }
            yield return null;
        }

        slow_button_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        //make sure transmit not active
        if (slow_button_coroutine == null)
        {
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], inputs))
            {
                slow_button_coroutine = StartCoroutine(slowButton());
            }
        }
        if (frequency_adjustment_coroutine == null)
        {
            for (int i = 1; i <= 2; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    BUTTONS[i].toggle(0.1f);
                    if (i == 1)
                    {
                        BUTTONS[2].updateInteractable(false);
                    }
                    else
                    {
                        BUTTONS[1].updateInteractable(false);
                    }
                    int new_freq = transform.gameObject.GetComponent<TransmissionHandler>().updateFrequency(i - 1);
                    transmitFrequencyAdjustmentRPC(i - 1, new_freq);
                    return;
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitSlowButtonChangeRPC(bool sba)
    {
        slow_button_active = sba;
        transform.gameObject.GetComponent<TransmissionHandler>().adjustSpeed(slow_button_active);
    }

    [Rpc(SendTo.Everyone)]
    private void transmitSlowButtonPositionRPC(float sbp)
    {
        slow_button_percentage = sbp;
        displaySlowButtonPositionAdjustment();
    }

    [Rpc(SendTo.Everyone)]
    private void transmitFrequencyAdjustmentRPC(int index, int new_freq)
    {
        transform.gameObject.GetComponent<TransmissionHandler>().setFrequency(new_freq);
        if (frequency_adjustment_coroutine != null)
        {
            StopCoroutine(frequency_adjustment_coroutine);
        }
        frequency_adjustment_coroutine = StartCoroutine(adjustFrequency(index));
    }
}
