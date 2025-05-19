/*
    DestructCode.cs
    - Used to input the four-digit self-destruct code
    Contributor(s): Jake Schott
    Last Updated: 5/17/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DestructCode : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float PUSH_TIME = 0.2f;

    private string[] CONTROL_NAMES = new string[] { "SELF-DESTRUCT DIGIT A", "SELF-DESTRUCT DIGIT B", "SELF-DESTRUCT DIGIT C", "SELF-DESTRUCT DIGIT D"};
    private List<string> CONTROL_DESCS = new List<string> { "DECREASE", "INCREASE" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject destruct_display;
    public List<GameObject> buttons; // 0-3 is up, 4-7 is down

    private Vector3[] initial_pos = new Vector3[8];
    private Vector3 push_direction = new Vector3(0.0019f, -0.0051f, 0f);

    private int[] code = new int[] { 0, 0, 0, 0 };
    private Coroutine digit_adjustment_coroutine = null;

    private List<string> ray_targets = new List<string> { "destruct_digit_a", "destruct_digit_b", "destruct_digit_c", "destruct_digit_d"};

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, true));
        hud_info.setButtons(BUTTONS);

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

        return hud_info;
    }

    IEnumerator digitAdjustment(int index)
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

            if (i == 0)
            {
                displayCodeAdjustment();
            }
        }

        BUTTONS[0].updateInteractable(true);
        BUTTONS[1].updateInteractable(true);

        digit_adjustment_coroutine = null;
    }

    private void displayCodeAdjustment()
    {
        for (int i = 1; i <= 4; i++)
        {
            destruct_display.transform.GetChild(i).GetComponent<TMP_Text>().SetText(code[i - 1].ToString());
        }
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        int digit = ray_targets.IndexOf(current_target.name);
        
        if (digit_adjustment_coroutine == null)
        {
            for (int i = 0; i <= 1; i++)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    int button_index = digit;
                    BUTTONS[i].toggle(0.1f);
                    if (i == 0)
                    {
                        button_index += 4;
                        code[digit]--;
                        if (code[digit] < 0)
                        {
                            code[digit] = 9;
                        }
                        BUTTONS[1].updateInteractable(false);
                    }
                    else
                    {
                        code[digit]++;
                        if (code[digit] > 9)
                        {
                            code[digit] = 0;
                        }
                        BUTTONS[0].updateInteractable(false);
                    }
                    transmitDigitChangeRPC(button_index, code[0], code[1], code[2], code[3]);
                    return;
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitDigitChangeRPC(int button_index, int a, int b, int c, int d)
    {
        code[0] = a;
        code[1] = b;
        code[2] = c;
        code[3] = d;
        if (digit_adjustment_coroutine != null)
        {
            StopCoroutine(digit_adjustment_coroutine);
        }
        digit_adjustment_coroutine = StartCoroutine(digitAdjustment(button_index));
    }
}
