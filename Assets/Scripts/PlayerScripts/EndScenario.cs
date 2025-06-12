using System.Collections;
using TMPro;
using UnityEngine;

public class EndScenario : MonoBehaviour
{
    public GameObject display_message;
    private Coroutine display_message_coroutine = null;
    IEnumerator messageDisplay(Color tc)
    {
        float display_time = 1.0f;
        while (display_time > 0.0f)
        {
            display_time = Mathf.Max(0.0f, display_time - Time.deltaTime);
            display_message.GetComponent<TMP_Text>().color = new Color(tc.r, tc.g, tc.b, 1.0f - display_time);

            yield return null;
        }
    }
    public void displayEndScenario(string msg, Color text_color)
    {
        display_message.GetComponent<TMP_Text>().SetText(msg);
        display_message.GetComponent<TMP_Text>().color = new Color(text_color.r, text_color.g, text_color.b, 0.0f);
        display_message.SetActive(true);

        if (display_message_coroutine != null)
        {
            StopCoroutine(display_message_coroutine);
        }
        StartCoroutine(messageDisplay(text_color));
    }
}
