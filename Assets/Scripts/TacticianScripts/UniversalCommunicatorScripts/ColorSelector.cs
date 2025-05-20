/*
    ColorSelector.cs
    - Handles color slider
    - Updates characters
    Contributor(s): Jake Schott
    Last Updated: 5/15/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ColorSelector : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    Color[] COLOR_OPTIONS = new Color[4] { new Color(0f, 0.84f, 1f), new Color(0.129f, 1f, 0.04f), new Color(0.69f, 0f, 0.69f), new Color(0.84f, 0.62f, 0f) };
    private static float MOVE_TIME = 0.5f;

    private string CONTROL_NAME = "COLOR SELECTOR";
    private List<string> CONTROL_DESCS = new List<string> { "SHIFT LEFT", "SHIFT RIGHT" };
    private List<int> CONTROL_INDEXES = new List<int>() {4, 5};
    private List<Button> BUTTONS = new List<Button>();

    public List<GameObject> character_displays = null;
    public GameObject selector_lever;
    public GameObject selector_canvas;
    private Vector3 initial_pos;
    private Vector3 final_pos = new Vector3(-0.4931f, -0.0132f, -17.8889f);
    private int curr_color = 0;

    private Coroutine color_shift_coroutine = null;

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
    public int getCurrColor()
    {
        return curr_color;
    }

    private void displayAdjustment()
    {
        for (int i = 0; i < 12; i++)
        {
            character_displays[i].transform.GetChild(1).gameObject.GetComponent<TMP_Text>().color = COLOR_OPTIONS[curr_color];
            character_displays[i].transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = COLOR_OPTIONS[curr_color];
        }
    }

    IEnumerator selectorShift()
    {
        float animation_time = MOVE_TIME;

        Vector3 starting_pos = selector_lever.transform.localPosition;
        Vector3 dest_pos =
            new Vector3(Mathf.Lerp(initial_pos.x, final_pos.x, curr_color / 3.0f),
                        Mathf.Lerp(initial_pos.y, final_pos.y, curr_color / 3.0f),
                        Mathf.Lerp(initial_pos.z, final_pos.z, curr_color / 3.0f));

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

        displayAdjustment();

        BUTTONS[0].updateInteractable(curr_color > 0);
        BUTTONS[1].updateInteractable(curr_color < 3);
        BUTTONS[0].untoggle();
        BUTTONS[1].untoggle();

        color_shift_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (color_shift_coroutine == null)
        {
            bool shifted = false;
            if (curr_color < 3)
            {
                if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], keys_down)) //shift right
                {
                    shifted = true;
                    BUTTONS[1].toggle();
                    BUTTONS[0].updateInteractable(false);
                    curr_color++;
                    transmitColorSelectionAdjustmentRPC(curr_color);
                }
            }
            if (shifted == false)
            {
                if (curr_color > 0)
                {
                    if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], keys_down)) //shift left
                    {
                        BUTTONS[0].toggle();
                        BUTTONS[1].updateInteractable(false);
                        curr_color--;
                        transmitColorSelectionAdjustmentRPC(curr_color);
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitColorSelectionAdjustmentRPC(int co)
    {
        curr_color = co;
        if (color_shift_coroutine != null)
        {
            StopCoroutine(color_shift_coroutine);
        }
        color_shift_coroutine = StartCoroutine(selectorShift());
    }
}
