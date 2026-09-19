/*
    ControlConsoleSpaceDoors.cs
    - Handles inputs for space doors lever
    Contributor(s): Jake Schott
    Last Updated: 9/18/2026
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlConsoleSpaceDoors : MonoBehaviour, IControllable, IIKTargetable
{
    //CLASS CONSTANTS
    private static float MOVE_SPEED = 35.0f;
    private Vector3 LEVER_FINAL_POS = new Vector3(0.0f, 0.135f, 0.027f);

    private string CONTROL_NAME = "SPACE DOORS";
    private static string INFO_MESSAGE = "Used to open space doors in ship hangar.";
    private List<string> CONTROL_DESCS = new List<string> { "CLOSE", "OPEN" };
    private List<int> CONTROL_INDEXES = new List<int>() { 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject space_doors_handle;
    public GameObject space_doors_handle_display; //used to display the bars on the side of the handle
    public GameObject space_doors_wall_display; //used to display the wall

    private float space_doors_open_percentage = 0.0f;

    private Coroutine space_doors_adjustment_coroutine = null;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private static HUDInfo hud_info = null;

    [Header("IK Targetable Details")]
    public GameObject IK_target;
    public AnimatorHandler.HandInteractionType hand_interaction_type = AnimatorHandler.HandInteractionType.Grasp;
    public float hand_pose = 0;
    public bool does_right_hand_flip = false;
    public Vector3 right_hand_offset = Vector3.zero;
    [Tooltip("Set to -1 for no lerp")]
    public float lerp_speed = 5f;

    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], false, false)); //close button
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false)); //open button
        hud_info.setButtons(BUTTONS);
        hud_info.setInfo(INFO_MESSAGE);
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

    public bool doorIsOpen()
    {
        return (space_doors_open_percentage == 1.0f);
    }

    private void displayAdjustment()
    {
        //update lever position
        space_doors_handle.transform.localPosition = Vector3.Lerp(Vector3.zero, LEVER_FINAL_POS, space_doors_open_percentage);

        //update lever display
        for (int i = 0; i < 2; i++)
        {
            space_doors_handle_display.transform.GetChild((i * 3) + 2).GetChild(0).gameObject.SetActive(space_doors_open_percentage < 1.0f);
            space_doors_handle_display.transform.GetChild((i * 3) + 3).GetComponent<UnityEngine.UI.Image>().fillAmount = space_doors_open_percentage;
            space_doors_handle_display.transform.GetChild((i * 3) + 4).GetChild(0).gameObject.SetActive(space_doors_open_percentage == 1.0f);
        }

        //update wall display
        string[] door_labels = new string[] { "TOP", "BOTTOM" };
        int rounded_percentage = Mathf.FloorToInt((1.0f - space_doors_open_percentage) * 100.0f);
        for (int i = 0; i < 2; i++)
        {
            space_doors_wall_display.transform.GetChild(i + 2).GetComponent<UnityEngine.UI.Image>().fillAmount = (1.0f - space_doors_open_percentage);
            space_doors_wall_display.transform.GetChild(i + 2).GetChild(2).GetComponent<TMP_Text>().SetText(door_labels[i] + ": " + rounded_percentage + "%");
        }
        space_doors_wall_display.transform.GetChild(4).gameObject.SetActive(space_doors_open_percentage < 1.0f);
        space_doors_wall_display.transform.GetChild(5).gameObject.SetActive(space_doors_open_percentage == 1.0f);

        //check for door unlock
        ReferenceAssistor.Instance.intro_sequence_manager.checkForDoorUnlock();
    }

    private bool checkIfChangeNecessary()
    {
        if (PrimaryScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && PrimaryScript.checkInputIndex(CONTROL_INDEXES[1], keys_down))
        {
            return false;
        }
        if (PrimaryScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && space_doors_open_percentage > 0.0f)
        {
            return true;
        }
        if (PrimaryScript.checkInputIndex(CONTROL_INDEXES[1], keys_down) && space_doors_open_percentage < 1.0f)
        {
            return true;
        }
        return false;
    }

    IEnumerator spaceDoorsAdjustment()
    {
        float momentum = 0.01f;
        while (checkIfChangeNecessary())
        {
            int impulse_direction = 0;
            if (PrimaryScript.checkInputIndex(CONTROL_INDEXES[1], keys_down) && space_doors_open_percentage < 1.0f) //E to increment
            {
                impulse_direction += 1;
            }
            if (PrimaryScript.checkInputIndex(CONTROL_INDEXES[0], keys_down) && space_doors_open_percentage > 0.0f)  //Q to decrement
            {
                impulse_direction -= 1;
            }
            if (impulse_direction != 0)
            {
                float dt = Mathf.Min(1.0f / 30.0f, Time.deltaTime);
                if (impulse_direction > 0)
                {
                    space_doors_open_percentage = Mathf.Min(1.0f, space_doors_open_percentage + (dt * MOVE_SPEED * 0.003f * momentum));
                }
                else
                {
                    space_doors_open_percentage = Mathf.Max(0.0f, space_doors_open_percentage - (dt * MOVE_SPEED * 0.003f * momentum));
                }

                momentum = Mathf.Min(1.1f, momentum + (dt * (1.1f)));

                displayAdjustment();
            }
            else
            {
                momentum = 0.01f;
            }

            BUTTONS[0].updateInteractable(space_doors_open_percentage > 0.0f);
            BUTTONS[1].updateInteractable(space_doors_open_percentage < 1.0f);

            keys_down.Clear();

            int iterator = 0; //counts frames
            while (keys_down.Count == 0 && iterator < 2)
            {
                yield return null;
                iterator++;
            }
        }

        space_doors_adjustment_coroutine = null;
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        keys_down = inputs;
        if (space_doors_adjustment_coroutine == null)
        {
            if (checkIfChangeNecessary())
            {
                space_doors_adjustment_coroutine = StartCoroutine(spaceDoorsAdjustment());
            }
        }
    }
}