/*
    ShipManual.cs
    - Handles inputs for communicator keyboard
    - Displays to code screen
    Contributor(s): Jake Schott
    Last Updated: 5/19/2025
*/

using UnityEngine;
using System.Collections;
using TMPro;

public class ShipManual : Manual
{
    //CLASS CONSTANTS
    private static string[] WELCOME_MESSAGES = new string[]
    {
        "GOOD MORNING, CAPTAIN",
        "WELCOME BACK, CAPTAIN",
        "NICE TO SEE YOU, CAPTAIN"
    };
    private static float INTRO_TIME = 2.0f;

    public void Start()
    {
        manual_index = 0;
    }

    public int pickWelcomeMessage()
    {
        return (Random.Range(0, WELCOME_MESSAGES.Length));
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
        curr_screen = home_screen;
        currently_enabled = true;
        transform.GetComponent<ManualOnOff>().reactivate(manual_index);
        curr_button = home_screen.GetComponent<PanelInfo>().default_button;
        curr_button.GetComponent<IManualButton>().select();
        updateInteractableButtons();
        transform.GetComponent<ManualSelector>().activate(manual_index);

        power_on_coroutine = null;
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
            curr_screen.SetActive(false);
            welcome_screen.SetActive(false);
            if (curr_button != null)
            {
                curr_button.GetComponent<IManualButton>().deselect();
            }
            transform.GetComponent<ManualOnOff>().reactivate(manual_index);
            transform.GetComponent<ManualSelector>().deactivate(manual_index);
        }
    }
}