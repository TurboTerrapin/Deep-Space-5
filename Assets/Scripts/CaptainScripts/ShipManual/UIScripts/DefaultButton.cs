/*
    Default.cs
    - Default button
    Contributor(s): Jake Schott
    Last Updated: 5/18/2025
*/

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DefaultButton : ManualButton, IManualButton
{
    private static float FLASH_TIME = 2.0f;

    public GameObject selected_indicator;
    public Sprite selected;
    public Sprite unselected;

    private Coroutine highlight = null;

    IEnumerator highlightLoop()
    {
        GameObject background = selected_indicator.transform.GetChild(0).gameObject;
        background.GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0.44f, 0.53f, 0.0f);
        while (true)
        {
            for (int i = 0; i <= 1; i++)
            {
                float flash_time = FLASH_TIME * 0.5f;
                while (flash_time > 0.0f)
                {
                    float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
                    flash_time -= dt;

                    float alpha = (flash_time / (FLASH_TIME * 0.5f)) * 0.35f;
                    if (i == 1)
                    {
                        alpha = (1.0f - (flash_time / (FLASH_TIME * 0.5f))) * 0.35f;
                    }
                    background.GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0.44f, 0.53f, alpha);

                    yield return null;
                }
            }
        }
    }

    public void select()
    {
        selected_indicator.transform.GetComponent<UnityEngine.UI.Image>().sprite = selected;
        selected_indicator.transform.GetChild(1).GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        if (highlight != null)
        {
            StopCoroutine(highlight);
        }
        highlight = StartCoroutine(highlightLoop());
    }

    public void deselect()
    {
        selected_indicator.transform.GetComponent<UnityEngine.UI.Image>().sprite = unselected;
        if (highlight != null)
        {
            StopCoroutine(highlight);
        }
        selected_indicator.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0.44f, 0.53f, 0.0f);
        selected_indicator.transform.GetChild(1).GetComponent<TMP_Text>().fontStyle = FontStyles.Normal;
    }
}
