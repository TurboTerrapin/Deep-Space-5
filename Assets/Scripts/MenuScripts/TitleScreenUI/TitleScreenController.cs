using UnityEngine;
using TMPro;
using System.Collections;

public class TitleScreenController : MonoBehaviour
{
    //TitleScreen
    public TextMeshProUGUI PressStartText;
    public float FadeDuration = 1.5f; // Time for a full fade in/out
    public GameObject TitleScreenCanvas;

    //MainMenu
    public GameObject MainMenu;

    void Start()
    {
        TitleScreenCanvas.SetActive(true);
        StartCoroutine(FadeText());
        MainMenu.SetActive(false);
    }

    // Call SwitchCanvas() if any key is pressed
    void Update()
    {
        if (Input.anyKeyDown)
        {
            SwitchToMainMenu();
        }
    }

    IEnumerator FadeText()
    {
        while (true)
        {
            yield return StartCoroutine(FadeTo(0f, FadeDuration)); // Fade out
            yield return StartCoroutine(FadeTo(1.3f, FadeDuration)); // Fade in
        }
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        Color color = PressStartText.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            PressStartText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        PressStartText.color = new Color(color.r, color.g, color.b, targetAlpha);
    }

    void SwitchToMainMenu()
    {
         TitleScreenCanvas.SetActive(false);
         MainMenu.SetActive(true);
    }
}
