/*
    BriefingConsoleOptions.cs
    - Used to flip pages on briefing console display
    Contributor(s): Jake Schott
    Last Updated: 9/7/2026
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BriefingConsoleOptions : MonoBehaviour, IControllable, IIKTargetable
{
    //CLASS CONSTANTS
    private static Vector3 PUSH_ADJUSTMENT = new Vector3(0.0027f, 0.0f, -0.0064f);
    private static float PUSH_TIME = 0.25f;

    private string CONTROL_NAME = "BRIEFING CONSOLE";
    private List<string> CONTROL_DESCS = new List<string> { "BACK", "NEXT" };
    private List<int> CONTROL_INDEXES = new List<int> { 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public List<GameObject> briefing_console_buttons;
    public AudioSource briefing_console_selection_boop;
    public GameObject briefing_console_facial_recognition_display;
    private BriefingConsoleScreen briefing_console_screen;
    private BriefingConsolePower briefing_console_power;

    private bool is_active = false;
    private Coroutine button_press_coroutine = null;

    private static HUDInfo hud_info;

    [Header("IK Targetable Details")]
    public List<GameObject> IK_targets;
    private GameObject current_IK_target;
    public AnimatorHandler.HandInteractionType hand_interaction_type;
    public float hand_pose = 0;
    public bool does_right_hand_flip = false;
    public int finger_position = 0;
    public Vector3 right_hand_offset = Vector3.zero;
    public float lerp_speed = 5f;

    private void Start()
    {
        current_IK_target = IK_targets[0];
        briefing_console_screen = GetComponent<BriefingConsoleScreen>();
        briefing_console_power = GetComponent<BriefingConsolePower>();

        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], false, true));

        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setButtons(BUTTONS);
    }

    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    public Transform getIKTarget(GameObject current_target)
    {
        return current_IK_target.transform;
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

    public void activate()
    {
        is_active = true;
        updateButtons();
    }

    public void deactivate()
    {
        is_active = false;
        updateButtons();
    }

    private void updateButtons()
    {
        BUTTONS[0].updateInteractable(is_active && briefing_console_screen.getCurrentPage() > 0 && button_press_coroutine == null);
        BUTTONS[1].updateInteractable(is_active && (briefing_console_screen.getCurrentPage() < (BriefingConsoleScreen.PAGE_MESSAGES.Length - 1)) && button_press_coroutine == null);
    }

    IEnumerator buttonPress(int index)
    {
        current_IK_target = IK_targets[1 + index];
        briefing_console_power.deactivate();

        float anim_time = PUSH_TIME;
        for (int i = 0; i < 2; i++)
        {
            float half_time = PUSH_TIME * 0.5f;
            float push_time = half_time;

            while (push_time > 0.0f)
            {
                float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
                push_time = Mathf.Max(0.0f, push_time - dt);

                float push_percentage = (push_time / half_time);
                if (i == 0)
                {
                    push_percentage = 1.0f - push_percentage;
                }

                briefing_console_buttons[index].transform.localPosition = Vector3.Lerp(Vector3.zero, PUSH_ADJUSTMENT, push_percentage);

                yield return null;
            }

            if (i == 0)
            {
                briefing_console_selection_boop.Play();
                if (index == 0) //back button
                {
                    briefing_console_screen.back();
                }
                else //next button
                {
                    briefing_console_screen.next();
                }
            }
        }

        current_IK_target = IK_targets[0];
        briefing_console_power.activate();
        button_press_coroutine = null;
        updateButtons();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        if (briefing_console_screen.getEnabled() == false || is_active == false || button_press_coroutine != null)
        {
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            if (BUTTONS[i].getInteractable() == true && PrimaryScript.checkInputIndex(CONTROL_INDEXES[i], inputs))
            {
                BUTTONS[i].toggle(0.1f);
                button_press_coroutine = StartCoroutine(buttonPress(i));
                return;
            }
        }
    }
}