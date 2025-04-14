/*
    ControlScript.cs
    - Manages the HUD display for control interaction
    - Sends user inputs to control script if looking at said control and within RAYCAST_RANGE
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Windows;
using Unity.VisualScripting;
using UnityEngine.Device;
using UnityEngine.UIElements.Experimental;
using System;
using UnityEngine.Android;

public class ControlScript : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float RAYCAST_RANGE = 1.25f;

    //GAME OBJECTS
    public GameObject cursor; //the diamond in the center of the screen
    public GameObject control_info; //UI indicator that you are looking at a control
    public GameObject title; //title at the top of the UI indicator
    public GameObject buttons_panel; //contains all the buttons/dividers inside the trapezoid
    public GameObject pause_menu;
    public GameObject settings_menu;
    public GameObject script_holder; //empty GameObject that contains all the control scripts as components
    public Camera my_camera; //player's camera

    //CLASS VARIABLES
    private HUDInfo current_info;

    //SETTINGS
    private int HUD_setting = 0; //0 is Default, 1 is Minimized, 2 is Cursor Only, 3 is None
    private bool paused = false;

    //CONTROL INFO
    public List<string> collider_names = null; //names of the GameObjects that contain the colliders that the Raycast must hit 
    public List<string> corresponding_scripts = null; //names of the scripts that correspond to the GameObject colliders (ex. lever_target corresponds to Throttle)

    //INPUT INFO
    public static List<KeyCode[]> input_options = new List<KeyCode[]>{ 
        new KeyCode[] {KeyCode.W, KeyCode.UpArrow}, //first argument is displayed, others are not
        new KeyCode[] {KeyCode.A, KeyCode.LeftArrow},
        new KeyCode[] {KeyCode.S, KeyCode.DownArrow},
        new KeyCode[] {KeyCode.D, KeyCode.RightArrow},
        new KeyCode[] {KeyCode.Q, KeyCode.LeftArrow},
        new KeyCode[] {KeyCode.E, KeyCode.RightArrow},
        new KeyCode[] {KeyCode.Mouse0, KeyCode.KeypadEnter},
        new KeyCode[] {KeyCode.Alpha1, KeyCode.Keypad1},
        new KeyCode[] {KeyCode.Alpha2, KeyCode.Keypad2},
        new KeyCode[] {KeyCode.Alpha3, KeyCode.Keypad3},
        new KeyCode[] {KeyCode.Alpha4, KeyCode.Keypad4},
    };

    void Start()
    {
        control_info.SetActive(false); //hide UI indicator to start
    }

    //used to instantiate buttons/list entries for either trapezoid or minimized list
    private void createButtons()
    {
        //hide both UI indicators
        control_info.transform.GetChild(0).gameObject.SetActive(false); //make the trapezoid invisible
        control_info.transform.GetChild(1).gameObject.SetActive(false); //make the list visible

        //clear trapezoid
        for (int i = control_info.transform.GetChild(0).GetChild(0).childCount - 1; i >= 4; i--)
        {
            GameObject to_destroy = control_info.transform.GetChild(0).GetChild(0).GetChild(i).gameObject;
            UnityEngine.Object.Destroy(to_destroy);
        }

        //clear list
        for (int i = control_info.transform.GetChild(1).childCount - 1; i >= 1; i--)
        {
            GameObject to_destroy = control_info.transform.GetChild(1).GetChild(i).gameObject;
            UnityEngine.Object.Destroy(to_destroy);
        }

        if (HUD_setting < 2)
        {
            control_info.transform.GetChild(HUD_setting).gameObject.SetActive(true);

            GameObject frame = control_info.transform.GetChild(1).gameObject;
            if (HUD_setting == 0)
            {
                frame = control_info.transform.GetChild(0).GetChild(0).gameObject;
            }

            for (int i = 0; i < current_info.numOptions(); i++)
            {
                current_info.getButtons()[i].createVisual(HUD_setting, current_info.numOptions(), i, frame);
            }
        }
    }

    //used to update buttons that may no longer be interactable
    private void updateButtons(HUDInfo temp_info)
    {
        for (int b = 0; b < current_info.numOptions(); b++)
        {
            current_info.getButtons()[b].updateInteractable(temp_info.getButtons()[b].isInteractable());
        }
    }

    //used by settings
    public void setHUD(int new_hud)
    {
        if (new_hud != HUD_setting)
        {
            if (new_hud >= 0 && new_hud < 4)
            {
                HUD_setting = new_hud;
            }
            if (HUD_setting != 0)
            {
                control_info.transform.GetChild(0).gameObject.SetActive(false); //make the trapezoid invisible
            }
            if (HUD_setting != 1)
            {
                control_info.transform.GetChild(1).gameObject.SetActive(false); //make the list invisible
            }
            title.GetComponent<TMP_Text>().SetText(""); //forces an update
        }
    }
    public bool isPaused()
    {
        return paused;
    }
    public void pause()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        pause_menu.SetActive(true);
        settings_menu.SetActive(false);
        paused = true;
        cursor.SetActive(false);
    }
    public void unpause()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        pause_menu.SetActive(false);
        settings_menu.SetActive(false);
        paused = false;
        if (HUD_setting != 3)
        {
            cursor.SetActive(true);
        }
    }

    void Update()
    {
        if (!paused)
        {
            if (Physics.Raycast(new Ray(my_camera.transform.position, my_camera.transform.forward), out RaycastHit hit, RAYCAST_RANGE)) //cast ray
            {
                if (hit.collider.gameObject.layer == 6) //the ray hit a control (Layer 6 = Control)
                {
                    IControllable target_control =
                        (IControllable)script_holder.GetComponent(corresponding_scripts[collider_names.IndexOf(hit.collider.gameObject.name)]); //get corresponding class
                    
                    HUDInfo temp_info = target_control.getHUDinfo(hit.collider.gameObject);

                    if (title.GetComponent<TMP_Text>().text.CompareTo(temp_info.getName()) != 0 || current_info.numOptions() != temp_info.numOptions())
                    {
                        title.GetComponent<TMP_Text>().SetText(temp_info.getName()); //set title of that control
                        current_info = temp_info;
                        if (HUD_setting < 2)
                        {
                            createButtons();
                        }
                    }
                    else
                    {
                        if (HUD_setting < 2)
                        {
                            updateButtons(temp_info);
                        }
                    }

                    List<KeyCode> current_inputs = new List<KeyCode>(); //gets all inputted keys
                    for (int b = 0; b < current_info.numOptions(); b++)
                    {
                        Button curr_button = current_info.getButtons()[b];
                        if (curr_button.isInteractable() == true)
                        {
                            bool pressed = false;
                            for (int i = 0; i < input_options[curr_button.getControlIndex()].Length; i++)
                            {
                                if (curr_button.isToggle() == false) { 
                                    if (UnityEngine.Input.GetKey(input_options[curr_button.getControlIndex()][i]))
                                    {
                                        pressed = true;
                                    }
                                }
                                else
                                {
                                    if (UnityEngine.Input.GetKeyDown(input_options[curr_button.getControlIndex()][i]))
                                    {
                                        curr_button.toggle();
                                        pressed = true;
                                    }
                                }
                                if (pressed == true)
                                {
                                    current_inputs.Add(input_options[curr_button.getControlIndex()][i]);
                                    curr_button.highlight(Time.deltaTime);
                                    break;
                                }
                            }
                            if (pressed == false)
                            {
                                curr_button.darken(Time.deltaTime);
                            }
                        }
                    }
                    control_info.SetActive(true); //show UI indicator
                    target_control.handleInputs(current_inputs, hit.collider.gameObject, 1); //call when all inputs have been checked
                    return;
                }
            }
        }
        control_info.SetActive(false); //hide UI indicator if not looking at a control
        title.GetComponent<TMP_Text>().SetText(""); //forces an update if not looking at a control
    }
}
