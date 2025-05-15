/*
    TorpedoSelector.cs
    - Handles torpedo slider
    - Updates arrow screen
    Contributor(s): Jake Schott
    Last Updated: 5/15/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TorpedoSelector : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float MOVE_TIME = 0.5f;

    private string CONTROL_NAME = "TORPEDO SELECTOR";
    private List<string> CONTROL_DESCS = new List<string>{"SHIFT LEFT", "SHIFT RIGHT"};
    private List<int> CONTROL_INDEXES = new List<int>(){4, 5};
    private List<Button> BUTTONS = new List<Button>();

    public GameObject selector_lever;
    public GameObject selector_canvas;
    private Vector3 initial_pos;
    private Vector3 final_pos = new Vector3(-0.08192f, -0.01329f, -17.8889f);
    private int torpedo_option = 0;

    private Coroutine torpedo_shift_coroutine = null;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, true));
        hud_info.setButtons(BUTTONS);

        initial_pos = selector_lever.transform.localPosition;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    IEnumerator selectorShift()
    {
        for (int i = 5; i <= 8; i++)
        {
            selector_canvas.transform.GetChild(i).gameObject.SetActive(false);
        }

        float animation_time = MOVE_TIME;

        Vector3 starting_pos = selector_lever.transform.localPosition;
        Vector3 dest_pos =
            new Vector3(Mathf.Lerp(initial_pos.x, final_pos.x, torpedo_option / 3.0f),
                        Mathf.Lerp(initial_pos.y, final_pos.y, torpedo_option / 3.0f),
                        Mathf.Lerp(initial_pos.z, final_pos.z, torpedo_option / 3.0f));

        //move slider
        while (animation_time > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            animation_time = Mathf.Max(0.0f, animation_time - dt);
            selector_lever.transform.localPosition =
                new Vector3(Mathf.Lerp(starting_pos.x, dest_pos.x, 1.0f - (animation_time / MOVE_TIME)),
                            Mathf.Lerp(starting_pos.y, dest_pos.y, 1.0f - (animation_time / MOVE_TIME)),
                            Mathf.Lerp(starting_pos.z, dest_pos.z, 1.0f - (animation_time / MOVE_TIME)));

            yield return null;
        }

        selector_canvas.transform.GetChild(torpedo_option + 5).gameObject.SetActive(true);

        BUTTONS[0].updateInteractable(torpedo_option > 0);
        BUTTONS[1].updateInteractable(torpedo_option < 3);
        BUTTONS[0].untoggle();
        BUTTONS[1].untoggle();

        torpedo_shift_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (torpedo_shift_coroutine == null)
        {
            bool shifted = false;
            if (torpedo_option < 3)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)) //shift right
                {
                    shifted = true;
                    BUTTONS[1].toggle();
                    BUTTONS[0].updateInteractable(false);
                    torpedo_option++;
                    transmitTorpedoSelecitonAdjustmentRPC(torpedo_option);
                }
            }
            if (shifted == false)
            {
                if (torpedo_option > 0)
                {
                    if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down)) //shift left
                    {
                        BUTTONS[0].toggle();
                        BUTTONS[1].updateInteractable(false);
                        torpedo_option--;
                        transmitTorpedoSelecitonAdjustmentRPC(torpedo_option);
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitTorpedoSelecitonAdjustmentRPC(int to)
    {
        torpedo_option = to;
        if (torpedo_shift_coroutine != null)
        {
            StopCoroutine(torpedo_shift_coroutine);
        }
        torpedo_shift_coroutine = StartCoroutine(selectorShift());
    }
}
