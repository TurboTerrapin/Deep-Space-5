/*
    CommunicationsManual.cs

    Contributor(s): Jake Schott
    Last Updated: 5/22/2025
*/

using UnityEngine;
using System.Collections;
using TMPro;

public class CommunicationsManual : Manual
{
    //CLASS CONSTANTS
    private static float INTRO_TIME = 2.0f;

    public void Start()
    {
        manual_index = 1;
    }

    IEnumerator powerOn()
    {
        welcome_screen.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().fillAmount = 0.0f;
        welcome_screen.transform.GetChild(1).GetComponent<TMP_Text>().SetText("LOADING");
        welcome_screen.SetActive(true);

        float animation_time = INTRO_TIME * 0.8f;
        while (animation_time > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            animation_time -= dt;

            welcome_screen.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().fillAmount = 1.0f - (animation_time / (INTRO_TIME * 0.8f));
            yield return null;
        }
        welcome_screen.transform.GetChild(1).GetComponent<TMP_Text>().SetText("DONE");

        yield return new WaitForSeconds(INTRO_TIME * 0.2f);

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

    public void powerSwitch(bool to_switch_to)
    {
        if (to_switch_to == true)
        {
            if (power_on_coroutine != null)
            {
                StopCoroutine(power_on_coroutine);
            }
            power_on_coroutine = StartCoroutine(powerOn());
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