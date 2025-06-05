/*
    ResetDisplay.cs
    - Clears code
    Contributor(s): Jake Schott
    Last Updated: 5/16/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResetDisplay : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float PUSH_TIME = 0.5f;

    private string CONTROL_NAME = "RESET DISPLAY";
    private List<string> CONTROL_DESCS = new List<string> {"RESET"};
    private List<int> CONTROL_INDEXES = new List<int>() {6};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject reset_button;

    private Vector3 initial_pos;
    private Vector3 final_pos = new Vector3(-3.1877f, 8.7356f, 3.7738f);

    private Coroutine reset_display_coroutine = null;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        hud_info.setButtons(BUTTONS);

        //set initial positions
        initial_pos = reset_button.transform.localPosition;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    IEnumerator resetDisplay()
    {
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

                reset_button.transform.localPosition =
                    new Vector3(Mathf.Lerp(initial_pos.x, final_pos.x, push_percentage),
                                Mathf.Lerp(initial_pos.y, final_pos.y, push_percentage),
                                Mathf.Lerp(initial_pos.z, final_pos.z, push_percentage));

                yield return null;
            }

            if (i == 0)
            {
                transform.gameObject.GetComponent<UniversalCommunicator>().updateDisplay();
            }
        }

        BUTTONS[0].updateInteractable(true);

        reset_display_coroutine = null;
    }

    public void pushResetButton()
    {
        if (reset_display_coroutine != null)
        {
            StopCoroutine(reset_display_coroutine);
        }
        StartCoroutine(resetDisplay());
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        //check map config button
        if (reset_display_coroutine == null)
        {
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], inputs))
            {
                BUTTONS[0].toggle(0.2f);
                transform.gameObject.GetComponent<UniversalCommunicator>().resetDisplay();
            }
        }
    }
}
