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
            home_screen.SetActive(false);
            welcome_screen.SetActive(false);
            transform.GetComponent<ShipManualOnOff>().reactivate();
        }
    }
}