/*
    Button.cs
    - Stores information for a button 
    Contributor(s): Jake Schott
    Last Updated: 4/16/2025
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
public class Button
{
    //CLASS CONSTANTS
    private static float COLOR_CHANGE_FACTOR = 10.0f;
    private static Color DARK_GRAY = new Color(0.22f, 0.22f, 0.22f, 1.0f); //default color
    private static Color LIGHT_BLUE = new Color(0f, 0.42f, 0.51f, 1.0f); //being pressed

    //BUTTON LAYOUT INFORMATION
    private static List<float[]> default_x_positions = new List<float[]>
    {
        new float[] {0f}, //1 button
        new float[] {-76f, 76f}, //2 buttons
        new float[] {-114f, 0f, 114f}, //3 buttons
        new float[] {-138f, -46f, 46f, 138f} //4 buttons
    };

    private static List<int[]> button_templates = new List<int[]>
    {
        new int[] {0},
        new int[] {1, 1},
        new int[] {1, 2, 1},
        new int[] {1, 2, 2, 1}
    };

    private static Vector2[] button_sizes = 
    {
        new Vector2(150f, 20f),
        new Vector2(150f, 20f),
        new Vector2(112f, 20f),
        new Vector2(90f, 20f)
    };

    private static List<float[]> divider_positions = new List<float[]>
    {
        new float[] {}, //1 button
        new float[] {0f}, //2 buttons
        new float[] {-57f, 57f}, //3 buttons
        new float[] {-92f, 0f, 92f} //4 buttons
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
    public bool isInteractable()
    {
        return interactable;
    }
    public bool isToggle()
    {
        return toggle_only;
    }
    public void showProgress(float remaining_percentage)
    {
        if (fill_button != null)
        {
            fill_button.GetComponent<UnityEngine.UI.Image>().fillAmount = remaining_percentage;
        }
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
            if (visual_button.transform.childCount > 0) //default view
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
            if (visual_button.transform.childCount > 0) //ensures default view only
            {
                if (this.interactable == false)
                {
                    if (is_toggled == false)
                    {
                        visual_button.GetComponent<UnityEngine.UI.Image>().color = new Color(DARK_GRAY.r, DARK_GRAY.g, DARK_GRAY.b, 0.03f);
                        visual_button.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(1f, 1f, 1f, 0.02f);
                    }
                    else
                    {
                        if (fill_button == null)
                        {
                            fill_button = UnityEngine.Object.Instantiate(visual_button, visual_button.transform.parent);
                            fill_button.name = "FILL_" + visual_button.name;
                            fill_button.GetComponent<UnityEngine.UI.Image>().color = LIGHT_BLUE;
                            fill_button.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(1f, 1f, 1f, 1f);
                            fill_button.GetComponent<UnityEngine.UI.Image>().type = UnityEngine.UI.Image.Type.Filled;
                            fill_button.GetComponent<UnityEngine.UI.Image>().fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
                            fill_button.GetComponent<UnityEngine.UI.Image>().fillAmount = 1f;
                        }
                    }
                }
                else
                {
                    is_toggled = false;
                    visual_button.GetComponent<UnityEngine.UI.Image>().color = DARK_GRAY;
                    visual_button.transform.GetChild(0).GetComponent<TMP_Text>().color = Color.white;
                    if (fill_button != null)
                    {
                        UnityEngine.Object.Destroy(fill_button);
                    }
                }
            }
        }
    }
    public void updateTogglable(bool toggle_only)
    {
        this.toggle_only = toggle_only;
    }
    public void toggle()
    {
        is_toggled = true;
    }

    public void untoggle()
    {
        is_toggled = false;
    }

    public void createVisual(int HUD_setting, int num_buttons, int order_index, GameObject frame)
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
            //copy button
            visual_button = UnityEngine.Object.Instantiate(frame.transform.GetChild(button_templates[num_buttons - 1][order_index]).gameObject, frame.transform);

            //resize
            visual_button.GetComponent<RectTransform>().sizeDelta = button_sizes[num_buttons - 1];
            visual_button.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(button_sizes[num_buttons - 1].x - 16f, button_sizes[num_buttons - 1].y * 0.7f);

            //position
            visual_button.GetComponent<RectTransform>().anchoredPosition = new Vector3(default_x_positions[num_buttons - 1][order_index], 20f, 0f);

            //if furthest right button, then flip
            if (num_buttons > 1 && order_index == num_buttons - 1)
            {
                visual_button.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                visual_button.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }

            //add dividers (if necessary)
            if (num_buttons > 1 && order_index == 0)
            {
                for (int i = 0; i < num_buttons - 1; i++)
                {
                    //copy divider
                    GameObject divider = UnityEngine.Object.Instantiate(frame.transform.GetChild(3).gameObject, frame.transform);

                    //position
                    divider.GetComponent<RectTransform>().anchoredPosition = new Vector3(divider_positions[num_buttons - 1][i], 20f, 0f);

                    divider.name = "DIVIDER" + i;
                    divider.SetActive(true);
                }
            }

            //make transparent if non-interactable
            if (interactable == false)
            {
                updateInteractable(false);
            }

            //set text info
            visual_button.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().SetText(button_desc + " (" + key + ")"); //set desc of that control
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
    private void updateColor()
    {
        visual_button.GetComponent<UnityEngine.UI.Image>().color =
                new Color(DARK_GRAY.r * (1 - percent_blue),
                          DARK_GRAY.g + (LIGHT_BLUE.g - DARK_GRAY.g) * percent_blue,
                          DARK_GRAY.b + (LIGHT_BLUE.b - DARK_GRAY.b) * percent_blue);
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
