/*
    ShipManual.cs
    - Handles inputs for communicator keyboard
    - Displays to code screen
    Contributor(s): Jake Schott
    Last Updated: 5/17/2025
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class ShipManual : MonoBehaviour
{
    //CLASS CONSTANTS
    private static string[] WELCOME_MESSAGES = new string[]
    {
        "GOOD MORNING, CAPTAIN",
        "WELCOME BACK, CAPTAIN",
        "NICE TO SEE YOU, CAPTAIN"
    };
    private static float INTRO_TIME = 2.0f;

    public GameObject welcome_screen;
    public GameObject home_screen;
    public GameObject curr_screen;
    public GameObject curr_button;

    private bool[] interactable_options = new bool[6];
    private bool currently_enabled = false;
    private Coroutine power_on_coroutine = null;

    public int pickWelcomeMessage()
    {
        return (Random.Range(0, WELCOME_MESSAGES.Length));
    }
    public bool getCurrentlyEnabled()
    {
        return currently_enabled;
    }
    public bool getCurrentlyAnimating()
    {
        return (power_on_coroutine != null);
    }
    
    IEnumerator powerOn(int msg)
    {
        welcome_screen.transform.GetChild(0).GetComponent<TMP_Text>().SetText("");
        welcome_screen.SetActive(true);

        for (int i = 0; i < WELCOME_MESSAGES[msg].Length; i++)
        {
            welcome_screen.transform.GetChild(0).GetComponent<TMP_Text>().SetText(welcome_screen.transform.GetChild(0).GetComponent<TMP_Text>().text + WELCOME_MESSAGES[msg][i]);
            yield return new WaitForSeconds((INTRO_TIME * 0.5f) / WELCOME_MESSAGES[msg].Length);
        }

        yield return new WaitForSeconds(INTRO_TIME * 0.5f);

        welcome_screen.SetActive(false);
        home_screen.SetActive(true);
        currently_enabled = true;
        transform.GetComponent<ShipManualOnOff>().reactivate();
        curr_button = home_screen.GetComponent<PanelInfo>().default_button;
        curr_button.GetComponent<IManualButton>().select();
        updateInteractableButtons();
        transform.GetComponent<ShipManualSelector>().activate();

        power_on_coroutine = null;
    }

    public void switchButtons(int dir)
    {
        GameObject new_button = null;
        if (dir == 0) //up
        {
            new_button = curr_button.GetComponent<ManualButton>().up;
        }
        else if (dir == 1) //down
        {
            new_button = curr_button.GetComponent<ManualButton>().down;
        }
        else if (dir == 2)
        {
            new_button = curr_button.GetComponent<ManualButton>().left;
        }
        else if (dir == 3)
        {
            new_button = curr_button.GetComponent<ManualButton>().right;
        }
        if (new_button != null)
        {
            curr_button.GetComponent<IManualButton>().deselect();
            curr_button = new_button;
            curr_button.GetComponent<IManualButton>().select();
        }
        updateInteractableButtons();
    }

    public void updateInteractableButtons()
    {
        bool[] selector_options = new bool[6];
        if (curr_screen.GetComponent<PanelInfo>().back_panel != null)
        {
            selector_options[1] = true;
        }
        if (curr_button != null)
        {
            if (curr_button.GetComponent<ManualButton>().select_panel != null)
            {
                selector_options[0] = true;
            }
            if (curr_button.GetComponent<ManualButton>().up != null)
            {
                selector_options[2] = true;
            }
            if (curr_button.GetComponent<ManualButton>().down != null)
            {
                selector_options[3] = true;
            }
            if (curr_button.GetComponent<ManualButton>().left != null)
            {
                selector_options[4] = true;
            }
            if (curr_button.GetComponent<ManualButton>().right != null)
            {
                selector_options[5] = true;
            }
        }
        else
        {
            selector_options[2] = false;
            selector_options[3] = false;
            selector_options[4] = false;
            selector_options[5] = false;
        }
        interactable_options = selector_options;
    }

    public bool isValidInput(int input_index)
    {
        return (interactable_options[input_index]);
    }

    public bool[] getInteractableOptions()
    {
        return interactable_options;
    }

    public void back()
    {
        if (curr_screen.GetComponent<PanelInfo>().back_panel != null)
        {
            curr_button.GetComponent<IManualButton>().deselect();
            curr_screen.SetActive(false);
            curr_screen = curr_screen.GetComponent<PanelInfo>().back_panel;
            if (curr_screen.GetComponent<PanelInfo>().default_button != null)
            {
                curr_button = curr_screen.GetComponent<PanelInfo>().default_button;
                curr_button.GetComponent<IManualButton>().select();
            }
            else
            {
                curr_button = null;
            }
        }
        updateInteractableButtons();
    }

    public void forward()
    {
        if (curr_button.GetComponent<ManualButton>().select_panel != null)
        {
            curr_button.GetComponent<IManualButton>().deselect();
            curr_screen.SetActive(false);
            curr_screen = curr_button.GetComponent<ManualButton>().select_panel;
            if (curr_screen.GetComponent<PanelInfo>().default_button != null)
            {
                curr_button = curr_screen.GetComponent<PanelInfo>().default_button;
                curr_button.GetComponent<IManualButton>().select();
            }
            else
            {
                curr_button = null;
            }
        }
        updateInteractableButtons();
    }

    public void powerSwitch(bool to_switch_to, int msg)
    {
        if (to_switch_to == true)
        {
            if (power_on_coroutine != null)
            {
                StopCoroutine(power_on_coroutine);
            }
            power_on_coroutine = StartCoroutine(powerOn(msg));
        }
        else
        {
            currently_enabled = false;
            home_screen.SetActive(false);
            welcome_screen.SetActive(false);
            if (curr_button != null)
            {
                curr_button.GetComponent<IManualButton>().deselect();
            }
            transform.GetComponent<ShipManualOnOff>().reactivate();
            transform.GetComponent<ShipManualSelector>().deactivate();
        }
    }
}