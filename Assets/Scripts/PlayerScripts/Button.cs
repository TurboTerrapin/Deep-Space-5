/*
    Button.cs
    - Stores information for a button
    - Handles button, divider GUI
    Contributor(s): Jake Schott
    Last Updated: 5/6/2025
*/

/*
    ***READ ME!***
    
    This script handles UI layouts for buttons and the trapezoid. Every button is fed the layout index (LAYOUT #),
    the index of the button within that layout (left-to-right, top-bottom), and the corresponding frame (trapezoid or not).

    The layout descriptions are listed below:

    LAYOUT 0: 1 BUTTON, CENTERED (ex. character select)

    LAYOUT 1: 2 TOUCHING BUTTONS, BOTH CENTERED, DIVIDED BY A DIVIDER (ex. impulse throttle)

    LAYOUT 2: 3 BUTTONS, ALL SEPARATED (ex. inertial dampeners)

    LAYOUT 3: 3 BUTTONS, 1 SEPARATED ON LEFT, 2 TOUCHING ON RIGHT (ex. map options)

    LAYOUT 4: 4 BUTTONS, ALL SEPARATED (ex. hangar clamps)

    LAYOUT 5: 4 BUTTONS ALL CONNECTED BOTTOM ROW, 2 BUTTONS SEPARATED TOP ROW (ex. regulations manual)
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Button : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float COLOR_CHANGE_FACTOR = 10.0f;
    private static Color DARK_GRAY = new Color(0.22f, 0.22f, 0.22f, 1.0f); //default color
    private static Color LIGHT_BLUE = new Color(0f, 0.42f, 0.51f, 1.0f); //being pressed

    //BUTTON LAYOUT INFORMATION
    private static Vector2[] trapezoid_sizes = new Vector2[]
    {
        new Vector2(1100f, 250f),
        new Vector2(1100f, 250f),
        new Vector2(1100f, 250f),
        new Vector2(1100f, 250f),
        new Vector2(1100f, 250f),
        new Vector2(1600f, 350f)
    };

    private static List<Vector2[]> button_positions = new List<Vector2[]>
    {
        new Vector2[] {new Vector2(0f, -45f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
    };

    private static List<int[]> button_templates = new List<int[]>
    {
        new int[] {0},
        new int[] {0},
        new int[] {0},
        new int[] {0},
        new int[] {0},
        new int[] {0}
    };

    private static List<Vector2[]> button_sizes = new List<Vector2[]>
    {
        new Vector2[] {new Vector2(600f, 80f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)}
    };

    private static List<Vector2[]> divider_positions = new List<Vector2[]>
    {
        new Vector2[] {},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)},
        new Vector2[] {new Vector2(0f, 0f)}
    };

    //PRIVATE DATA MEMBERS
    private string button_desc; //ex. INCREASE
    private int control_index; //ex. 0 = KeyPad.W, based on array in ControlScript
    private bool interactable = true;
    private bool toggle_only = false;
    private bool is_toggled = false; //used to stay blue during toggles
    private GameObject visual_button;
    private GameObject fill_button;
    private float percent_blue = 0.0f;

    private void Start()
    {
        StartCoroutine(toggle_highlight());
    }

    IEnumerator toggle_highlight()
    {
        yield return new WaitForSecondsRealtime(1);
    }

    public Button(string button_desc, int control_index, bool interactable, bool toggle_only)
    {
        this.button_desc = button_desc;
        this.control_index = control_index;
        this.interactable = interactable;
        this.toggle_only = toggle_only;
    }
    public int getControlIndex()
    {
        return control_index;
    }
    public bool getInteractable()
    {
        return interactable;
    }
    public bool getTogglable()
    {
        return toggle_only;
    }
    public void updateDesc(string new_desc)
    {
        button_desc = new_desc;
        if (visual_button != null)
        {
            string key = ControlScript.input_options[control_index][0].ToString();
            if (key == "Mouse0")
            {
                key = "LMB";
            }
            if (key.Contains("Alpha"))
            {
                key = key.Substring(5);
            }
            if (visual_button.transform.childCount > 0) //trapezoid view
            {
                visual_button.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().SetText(button_desc + " (" + key + ")"); 
            }
            else //list view
            {
                visual_button.GetComponent<TMP_Text>().SetText(button_desc + " - " + key);
            }
        }
    }
    public void updateInteractable(bool interactable)
    {
        if (interactable != this.interactable)
        {
            percent_blue = 0.0f;
        }
        this.interactable = interactable;
        if (visual_button != null)
        { 
            if (visual_button.transform.childCount > 0) //trapezoid view
            {
                if (this.interactable == true)
                {
                    is_toggled = false;
                    visual_button.GetComponent<UnityEngine.UI.Image>().color = DARK_GRAY;
                    visual_button.transform.GetChild(2).GetComponent<TMP_Text>().color = Color.white;
            }
        }
    }
    public void toggle(float toggle_length)
    {
        is_toggled = true;
    }
    public void createVisual(int HUD_setting, int layout, int order_index, GameObject frame)
    {
        string key = ControlScript.input_options[control_index][0].ToString();
        if (key == "Mouse0")
        {
            key = "LMB";
        }
        if (key.Contains("Alpha"))
        {
            key = key.Substring(5);
        }

        //Default: Trapezoidal format
        if (HUD_setting == 0)
        {
            //define buttons panel
            GameObject buttons_panel = frame.transform.GetChild(4).gameObject;

            //copy button
            visual_button = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(0).gameObject, buttons_panel.transform);

            //resize
            visual_button.GetComponent<RectTransform>().sizeDelta = button_sizes[layout][order_index];
            visual_button.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(-1f * (button_sizes[layout][order_index].x / 2 + 20f), 0f, 0f);
            visual_button.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector3(button_sizes[layout][order_index].x / 2 + 20f, 0f, 0f);
            visual_button.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = button_sizes[layout][order_index];

            //handle rounded edges
            if (button_templates[layout][order_index] == 1) //left 
            {
                visual_button.transform.GetChild(0).GetComponent<UnityEngine.UIElements.Image>().image = null;
            }
            else if (button_templates[layout][order_index] == 2) //right
            {
                visual_button.transform.GetChild(1).GetComponent<UnityEngine.UIElements.Image>().image = null;
            }
            else if (button_templates[layout][order_index] == 3) //rectangle
            {
                visual_button.transform.GetChild(0).GetComponent<UnityEngine.UIElements.Image>().image = null;
                visual_button.transform.GetChild(1).GetComponent<UnityEngine.UIElements.Image>().image = null;
            }

            //position
            visual_button.GetComponent<RectTransform>().anchoredPosition = new Vector3(button_positions[layout][order_index].x, button_positions[layout][order_index].y, 0f);

            //handle trapezoid size, positioning, and dividers
            if (order_index == 0)
            {
                //trapezoid height/vertical position
                frame.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(0, trapezoid_sizes[layout].y);
                frame.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -1080f + trapezoid_sizes[layout].y / 2, 0f);
                //trapezoid center
                frame.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(trapezoid_sizes[layout].x, 0f);
                //trapezoid edge triangles
                frame.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(-1f * (trapezoid_sizes[layout].x / 2 + 75f), 0f, 0f);
                frame.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector3(trapezoid_sizes[layout].x / 2 + 75f, 0f, 0f);
                if (divider_positions[layout].Length > 0)
                {
                    for (int i = 0; i < divider_positions[layout].Length; i++)
                    {
                        //copy divider
                        GameObject divider = UnityEngine.Object.Instantiate(buttons_panel.transform.GetChild(3).gameObject, buttons_panel.transform);

                        //position
                        divider.GetComponent<RectTransform>().anchoredPosition = new Vector3(divider_positions[layout][i].x, divider_positions[layout][i].y, 0f);

                        divider.name = "DIVIDER" + i;
                        divider.SetActive(true);
                    }
                }
            }

            //make transparent if non-interactable
            if (interactable == false)
            {
                updateInteractable(false);
            }

            //set text info
            visual_button.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().SetText(button_desc + " (" + key + ")"); //set desc of that control
        }
        //Minimized: List format
        else if (HUD_setting == 1)
        {
            //copy button
            visual_button = UnityEngine.Object.Instantiate(frame.transform.GetChild(0).gameObject, frame.transform);

            //position button
            visual_button.GetComponent<RectTransform>().anchoredPosition = new Vector3(46f, (10f * order_index) + 8f, 0f);
            
            //set text info
            visual_button.GetComponent<TMP_Text>().SetText(button_desc + " - " + key);
        }
        if (visual_button != null)
        {
            visual_button.name = button_desc;
            visual_button.SetActive(true);
        }
    }
    
    //helper method 
    private void updateColor()
    {
        Color temp_color = 
            new Color(DARK_GRAY.r * (1 - percent_blue),
                      DARK_GRAY.g + (LIGHT_BLUE.g - DARK_GRAY.g) * percent_blue,
                      DARK_GRAY.b + (LIGHT_BLUE.b - DARK_GRAY.b) * percent_blue,
                      0.36f);
        visual_button.GetComponent<UnityEngine.UI.Image>().color = temp_color;
        visual_button.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = temp_color;
        visual_button.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().color = temp_color;
    }
    public void highlight(float delta_time)
    {
        if (visual_button != null)
        {
            percent_blue = Mathf.Min(1f, percent_blue + delta_time * COLOR_CHANGE_FACTOR);
            if (visual_button.transform.childCount > 0) //means default format
            {
                updateColor();
            }
        }
    }
    public void darken(float delta_time)
    {
        if (visual_button != null)
        {
            percent_blue = Mathf.Max(0f, percent_blue - delta_time * COLOR_CHANGE_FACTOR);
            if (visual_button.transform.childCount > 0) //means default format
            {
                updateColor();
            }
        }
    }
}
