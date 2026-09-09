/*
    BriefingConsolePower.cs
    - Used to turn on briefing console
    Contributor(s): Jake Schott
    Last Updated: 9/7/2026
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BriefingConsolePower : MonoBehaviour, IControllable, IIKTargetable
{
    //CLASS CONSTANTS
    private static Vector3 ON_POSITION = new Vector3(0.0168f, 0.0f, 0.0072f);
    private static float POWER_CHANGE_TIME = 0.5f;

    private string CONTROL_NAME = "BRIEFING CONSOLE";
    private List<string> CONTROL_DESCS = new List<string> { "ENABLE", "DISABLE" };
    private int CONTROL_INDEX = 6;
    private List<Button> BUTTONS = new List<Button>();

    public GameObject briefing_console_power_switch;
    public GameObject briefing_console_power_display;
    private BriefingConsoleScreen briefing_console_screen;

    private bool is_active = true;
    private Coroutine power_change_coroutine = null;

    private static HUDInfo hud_info;

    [Header("IK Targetable Details")]
    public GameObject IK_target;
    public AnimatorHandler.HandInteractionType hand_interaction_type;
    public float hand_pose = 0;
    public bool does_right_hand_flip = false;
    public int finger_position = 0;
    public Vector3 right_hand_offset = Vector3.zero;
    public float lerp_speed = 5f;

    private void Start()
    {
        briefing_console_screen = GetComponent<BriefingConsoleScreen>();

        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEX, true, true));

        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setButtons(BUTTONS);
    }

    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    public Transform getIKTarget(GameObject current_target)
    {
        return IK_target.transform;
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
        updateButton();
    }

    public void deactivate()
    {
        is_active = false;
        updateButton();
    }

    private void updateButton()
    {
        BUTTONS[0].updateInteractable(is_active && power_change_coroutine == null);
    }

    IEnumerator powerSwitch(bool to_switch_to)
    {
        if (to_switch_to == false)
        {
            for (int i = 0; i < 5; i++)
            {
                briefing_console_power_display.transform.GetChild(i + 1).GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.0f, 0.086f, 0.75f, 0.2f);
            }
            briefing_console_power_display.transform.GetChild(6).GetChild(0).gameObject.SetActive(false);
            briefing_console_screen.setEnabled(false);
        }

        float anim_time = POWER_CHANGE_TIME;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float switch_progress = anim_time / POWER_CHANGE_TIME;
            if (to_switch_to == true)
            {
                switch_progress = 1.0f - switch_progress;
            }
            briefing_console_power_switch.transform.localPosition = Vector3.Lerp(Vector3.zero, ON_POSITION, switch_progress);

            yield return null;
        }

        if (to_switch_to == true)
        {
            for (int i = 0; i < 5; i++)
            {
                briefing_console_power_display.transform.GetChild(i + 1).GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.0f, 0.086f, 0.75f);
                yield return new WaitForSeconds(0.1f);
            }
            briefing_console_power_display.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);

            BUTTONS[0].updateDesc(CONTROL_DESCS[1]);
            briefing_console_screen.setEnabled(true);
        }
        else
        {
            BUTTONS[0].updateDesc(CONTROL_DESCS[0]);
        }

        power_change_coroutine = null;
        updateButton();
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        if (is_active == false || power_change_coroutine != null)
        {
            return;
        }

        if (PrimaryScript.checkInputIndex(CONTROL_INDEX, inputs) == true)
        {
            BUTTONS[0].toggle(0.1f);
            power_change_coroutine = StartCoroutine(powerSwitch(!briefing_console_screen.getEnabled()));
        }
    }
}