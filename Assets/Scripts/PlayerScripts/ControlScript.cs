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
    private static float RAYCAST_RANGE = 2.0f;
    private static Color DARK_GRAY = new Color(0.22f, 0.22f, 0.22f, 1.0f); //default color
    private static Color LIGHT_BLUE = new Color(0f, 0.42f, 0.51f, 1.0f); //being pressed
    private static Color DARK_RED = new Color(0.7f, 0f, 0f); //unavailable

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
    private int num_options_displayed = 0; //how many controls the script currently is showing

    //SETTINGS
    private int HUD_setting = 0; //0 is Default, 1 is Minimized, 2 is Cursor Only, 3 is None
    private bool paused = false;

    //CONTROL INFO
    public List<string> collider_names = null; //names of the GameObjects that contain the colliders that the Raycast must hit 
    public List<string> corresponding_scripts = null; //names of the scripts that correspond to the GameObject colliders (ex. lever_target corresponds to Throttle)

    //INPUT INFO
    public static KeyCode[] primary_inputs = { //what is displayed (ex. Q is displayed, LeftArrow is not, they both work though) 
        KeyCode.W, 
        KeyCode.A, 
        KeyCode.S, 
        KeyCode.D, 
        KeyCode.Q, 
        KeyCode.E, 
        KeyCode.Mouse0
    }; 
    public static KeyCode[] secondary_inputs = { //what also works (ex. index 4 is both Q *AND* LeftArrow)
        KeyCode.UpArrow, //corresponds to W
        KeyCode.LeftArrow, //corresponds to A
        KeyCode.DownArrow, //corresponds to S
        KeyCode.RightArrow, //corresponds to D
        KeyCode.LeftArrow, //corresponds to Q
        KeyCode.RightArrow, //correponds to E
        KeyCode.KeypadEnter //correponds to Mouse0
    };

    void Start()
    {
        control_info.SetActive(false); //hide UI indicator to start
    }

    //used to set the text for trapezoid buttons
    private void setButtonInfo(GameObject button_to_set, HUDInfo buttons_info, int index)
    {
        string button_desc = buttons_info.getInputDescriptions()[index]; //ex. INCREASE
        string key = primary_inputs[buttons_info.getInputIndexes()[index]].ToString(); //ex. E
        if (key == "Mouse0")
        {
            key = "LMB";
        }
        button_to_set.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().SetText(button_desc + " (" + key + ")"); //set desc of that control
        button_to_set.name = //set name of button so it can be turned blue when pressed
            primary_inputs[buttons_info.getInputIndexes()[index]].ToString() + "_" + secondary_inputs[buttons_info.getInputIndexes()[index]].ToString() + "_";
        button_to_set.SetActive(true);
    }

    //used to instantiate buttons/list entries for either trapezoid or minimized list
    private void createButtons(HUDInfo buttons_info)
    {
        if (HUD_setting == 0) //Default: trapezoidal format
        {
            control_info.transform.GetChild(0).gameObject.SetActive(true); //make the trapezoid visible
            control_info.transform.GetChild(1).gameObject.SetActive(false); //make the list invisible
            foreach (Transform button in buttons_panel.transform) //destroy all existing buttons
            {
                if (button.gameObject.activeSelf == true)
                {
                    Destroy(button.gameObject); 
                }
            }
            if (buttons_info.numOptions() == 0)
            {
                //center red "unavailable" button
                GameObject unavailable_button = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(3).gameObject, buttons_panel.transform);
                unavailable_button.name = "Unavailable";
                unavailable_button.SetActive(true);
            }
            else if (buttons_info.numOptions() == 1) //only one button, middle
            {
                //center button
                GameObject new_button = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(0).gameObject, buttons_panel.transform);
                setButtonInfo(new_button, buttons_info, 0);
            }
            else if (buttons_info.numOptions() == 2) //two buttons, left and right
            {
                //left button
                GameObject left_button = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(1).gameObject, buttons_panel.transform);
                left_button.GetComponent<RectTransform>().anchoredPosition = new Vector3(-76f, 20f, 0f);
                setButtonInfo(left_button, buttons_info, 0);

                //right button
                GameObject right_button = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(1).gameObject, buttons_panel.transform);
                right_button.GetComponent<RectTransform>().anchoredPosition = new Vector3(76f, 20f, 0f);
                right_button.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                right_button.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                setButtonInfo(right_button, buttons_info, 1);
                
                //center divider
                GameObject divider = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(2).gameObject, buttons_panel.transform);
                divider.name = "Divider";
                divider.SetActive(true);
            }
        }
        else if (HUD_setting == 1) //Minimized: list format
        {
            control_info.transform.GetChild(0).gameObject.SetActive(false); //make the trapezoid invisible
            control_info.transform.GetChild(1).gameObject.SetActive(true); //make the list visible
            foreach (Transform list_entry in control_info.transform.GetChild(1)) //destroy all existing list entries
            {
                if (list_entry.gameObject.activeSelf == true)
                {
                    Destroy(list_entry.gameObject);
                }
            }
            for (int i = buttons_info.numOptions() - 1; i >= 0; i--)
            {
                GameObject new_entry = UnityEngine.Object.Instantiate(control_info.transform.GetChild(1).GetChild(0).gameObject, control_info.transform.GetChild(1));
                string button_desc = buttons_info.getInputDescriptions()[i];
                string key = primary_inputs[buttons_info.getInputIndexes()[i]].ToString();
                if (key == "Mouse0")
                {
                    key = "LMB";
                }
                new_entry.transform.gameObject.GetComponent<TMP_Text>().SetText(button_desc + " - " + key); //set desc of that control
                new_entry.GetComponent<RectTransform>().anchoredPosition = new Vector3(46f, (10f * i) + 8f, 0f);
                new_entry.SetActive(true);
            }
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
                    if (HUD_setting < 2)
                    {
                        HUDInfo temp_info = target_control.getHUDinfo();
                        if (title.GetComponent<TMP_Text>().text.CompareTo(temp_info.getName()) != 0 || num_options_displayed != temp_info.numOptions()) //checks if HUDInfo already loaded or if num of controls has changed
                        {
                            title.GetComponent<TMP_Text>().SetText(temp_info.getName()); //set title of that control
                            num_options_displayed = temp_info.numOptions();
                            createButtons(temp_info);
                        }
                        if (temp_info.numOptions() > 0)
                        {
                            foreach (Transform button in buttons_panel.transform) //make all buttons gray to start
                            {
                                if (button.name.CompareTo("Divider") != 0 && button.gameObject.activeSelf == true) //not a divider and is active
                                {
                                    button.gameObject.GetComponent<UnityEngine.UI.Image>().color = DARK_GRAY;
                                    button.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(1f, 1f, 1f);
                                }
                            }
                        }
                    }

                    List<KeyCode> current_inputs = new List<KeyCode>(); //gets all inputted keys
                    for (int i = 0; i < primary_inputs.Length; i++)
                    {
                        KeyCode input = KeyCode.None;
                        if (UnityEngine.Input.GetKey(primary_inputs[i])) //if key is down, add to input list
                        {
                            input = primary_inputs[i];
                        }
                        else if (UnityEngine.Input.GetKey(secondary_inputs[i])) //also valid
                        {
                            input = secondary_inputs[i];
                        }
                        if (input != KeyCode.None) //input was on the list
                        {
                            current_inputs.Add(input);
                            if (HUD_setting == 0)
                            {
                                foreach (Transform button in buttons_panel.transform)
                                {
                                    if (button.gameObject.name.Contains(input.ToString() + "_")) //if matching key in layout frame, then make blue
                                    {
                                        button.gameObject.GetComponent<UnityEngine.UI.Image>().color = LIGHT_BLUE;
                                        button.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(0.85f, 0.85f, 0.85f);
                                    }
                                }
                            }
                        }
                    }
                    control_info.SetActive(true); //show UI indicator
                    target_control.handleInputs(current_inputs); //call when all inputs have been checked
                    return;
                }
            }
        }
        control_info.SetActive(false); //hide UI indicator if not looking at a control
    }
}
