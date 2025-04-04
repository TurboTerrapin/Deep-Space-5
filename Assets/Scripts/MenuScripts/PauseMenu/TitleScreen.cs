using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkingText : MonoBehaviour
{
    public TextMeshProUGUI pressStartText;
    public float fadeDuration = 1.5f; // Time for a full fade in/out
    public GameObject titleScreenCanvas;
    public GameObject mainMenuCanvas;

    void Start()
    {
        StartCoroutine(FadeText());
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SwitchCanvas();
        }
    }

    IEnumerator FadeText()
    {
        while (true)
        {
            yield return StartCoroutine(FadeTo(0f, fadeDuration)); // Fade out
            yield return StartCoroutine(FadeTo(1.3f, fadeDuration)); // Fade in
        }
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        Color color = pressStartText.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            pressStartText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        pressStartText.color = new Color(color.r, color.g, color.b, targetAlpha);
    }

    void SwitchCanvas()
    {
        if (titleScreenCanvas != null)
        {
            titleScreenCanvas.SetActive(false);
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }
}
