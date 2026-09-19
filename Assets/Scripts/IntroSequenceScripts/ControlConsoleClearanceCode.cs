/*
    ControlConsoleClearanceCode.cs
    - Used to handle code input to unlock people door
    Contributor(s): Jake Schott
    Last Updated: 9/19/2026
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ControlConsoleClearanceCode : MonoBehaviour, IControllable, IIKTargetable
{
    //CLASS CONSTANTS
    private static float DIGIT_CHANGE_TIME = 0.2f;
    private static float CORRECT_CODE_FLASH_TIME = 0.15f;

    private string CONTROL_NAME = "CLEARANCE CODE";
    private static string INFO_MESSAGE = "Used to grant access to docked ship entry tunnel.";
    private List<string> CONTROL_DESCS = new List<string> { "DECREASE", "INCREASE" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject clearance_code_digit_display;
    public GameObject clearance_code_digit_switches;
    public AudioSource clearance_code_boop_sound;

    private int[] input_code = new int[] { 0, 0, 0, 0 };
    private Coroutine digit_adjustment_coroutine = null;
    private Coroutine correct_code_flasher_coroutine = null;

    private List<string> ray_targets = new List<string> { "clearance_code_digit_a", "clearance_code_digit_b", "clearance_code_digit_c", "clearance_code_digit_d" };

    private static HUDInfo hud_info = null;

    [Header("IK Targetable Details")]
    public List<GameObject> IK_targets = null;
    public AnimatorHandler.HandInteractionType hand_interaction_type = AnimatorHandler.HandInteractionType.Grasp;
    public float hand_pose = 0;
    public bool does_right_hand_flip = false;
    public Vector3 right_hand_offset = Vector3.zero;
    public float lerp_speed = 5f;

    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);

        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, true));
        hud_info.setButtons(BUTTONS);
        hud_info.setInfo(INFO_MESSAGE);
    }

    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    public Transform getIKTarget(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        return IK_targets[index].transform;
    }

    public AnimatorHandler.HandInteractionType getHandInteractionType()
    {
        return hand_interaction_type;
    }

    public float getHandPose()
    {
        return hand_pose;
    }

    public bool getRightHandFlip()
    {
        return does_right_hand_flip;
    }

    public Vector3 getRightHandOffset()
    {
        return right_hand_offset;
    }

    public float getLerpSpeed()
    {
        return lerp_speed;
    }

    public bool codeIsCorrect()
    {
        string correct_code = ReferenceAssistor.Instance.intro_sequence_manager.getClearanceCode();
        string current_code = "";
        for (int i = 0; i < 4; i++)
        {
            current_code += input_code[i].ToString();
        }

        return (correct_code.CompareTo(current_code) == 0);
    }

    IEnumerator digitAdjustment(int index, bool increase)
    {
        BUTTONS[0].updateInteractable(false);
        BUTTONS[1].updateInteractable(false);

        float initial_rotation = 12.0f;
        float destination_rotation = 42.0f;

        if (increase == true) //up
        {
            destination_rotation = -18.0f;
        }

        float anim_time = DIGIT_CHANGE_TIME;
        for (int i = 0; i < 2; i++)
        {
            float half_time = DIGIT_CHANGE_TIME * 0.5f;
            float curr_time = half_time;

            while (curr_time > 0.0f)
            {
                curr_time = Mathf.Max(0.0f, curr_time - Time.deltaTime);

                float switch_percentage = 1.0f - (curr_time / half_time);
                if (i == 1)
                {
                    switch_percentage = (curr_time / half_time);
                }

                clearance_code_digit_switches.transform.GetChild(index).localRotation = Quaternion.Euler(Mathf.Lerp(initial_rotation, destination_rotation, switch_percentage), 0.0f, 0.0f);

                yield return null;
            }

            if (i == 0)
            {
                clearance_code_boop_sound.Play();
                displayCodeAdjustment();
            }
        }

        BUTTONS[0].updateInteractable(true);
        BUTTONS[1].updateInteractable(true);

        digit_adjustment_coroutine = null;
    }

    private void digitsAlphaAdjustment(float a)
    {
        Color c = new Color(0.0f, 0.84f, 1.0f, a);
        for (int i = 0; i < 4; i++)
        {
            clearance_code_digit_display.transform.GetChild(i + 1).GetComponent<TMP_Text>().color = c;
        }
        for (int i = 0; i < 2; i++)
        {
            clearance_code_digit_display.transform.GetChild(i + 5).GetComponent<UnityEngine.UI.RawImage>().color = c;
        }
    }

    IEnumerator correctCodeFlasher()
    {
        float elapsed_time = 0.0f;
        while (true)
        {
            elapsed_time += Time.deltaTime;

            float a = Mathf.PingPong(elapsed_time, CORRECT_CODE_FLASH_TIME);

            if (a > CORRECT_CODE_FLASH_TIME / 2.0f)
            {
                a = 1.0f;
            }
            else
            {
                a = 0.2f;
            }

            digitsAlphaAdjustment(a);

            yield return null;
        }
    }

    private void checkCodeCorrectness()
    {
        //flash if correct code
        if (codeIsCorrect() && correct_code_flasher_coroutine == null)
        {
            correct_code_flasher_coroutine = StartCoroutine(correctCodeFlasher());
        }
        else
        {
            if (correct_code_flasher_coroutine != null)
            {
                StopCoroutine(correct_code_flasher_coroutine);
                correct_code_flasher_coroutine = null;
            }
            digitsAlphaAdjustment(1.0f);
        }
    }

    private void displayCodeAdjustment()
    {
        //update digits
        for (int i = 0; i < 4; i++)
        {
            clearance_code_digit_display.transform.GetChild(i + 1).GetComponent<TMP_Text>().SetText(input_code[i].ToString());
        }

        //check for correct code
        checkCodeCorrectness();

        //check for door unlock
        ReferenceAssistor.Instance.intro_sequence_manager.checkForDoorUnlock();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        int index = ray_targets.IndexOf(current_target.name);

        //digit adjustment
        if (digit_adjustment_coroutine == null)
        {
            for (int i = 0; i < 2; i++)
            {
                if (PrimaryScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
                {
                    BUTTONS[i].toggle(0.1f);
                    bool increase = true;
                    if (i == 0)
                    {
                        increase = false;
                        input_code[index]--;
                        if (input_code[index] < 0)
                        {
                            input_code[index] = 9;
                        }
                    }
                    else
                    {
                        input_code[index]++;
                        if (input_code[index] > 9)
                        {
                            input_code[index] = 0;
                        }
                    }
                    digit_adjustment_coroutine = StartCoroutine(digitAdjustment(index, increase));
                    return;
                }
            }
        }
    }
}