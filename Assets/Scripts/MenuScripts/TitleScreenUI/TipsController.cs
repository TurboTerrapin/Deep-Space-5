using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections;

public class TipsController : MonoBehaviour
{
    public GameObject[] Tips;

    private int CurrentTipIndex = 0;

    private Coroutine TipTimer;

    void Start()
    {
        UpdateCurrentTip();
        TipTimer = StartCoroutine(SwitchTip());
    }

    void UpdateCurrentTip()
    {
        for (int i = 0; i < Tips.Length; i++)
        {
            Tips[i].SetActive(false);
        }

        Tips[CurrentTipIndex].SetActive(true);

        TMP_Text text = Tips[CurrentTipIndex].GetComponentInChildren<TMP_Text>();
        Color color = text.color;

        text.color = new Color(color.r, color.g, color.b, 0f);

        StartCoroutine(FadeText(text, 1f, 1f));

        //Debug.Log(CurrentTipIndex);
    }

    public void PreviousTip()
    {
        CurrentTipIndex--;
        if (CurrentTipIndex < 0)
        {
            // wrap around
            CurrentTipIndex = Tips.Length - 1;
        }
        UpdateCurrentTip();
        RestartTipTimer();
    }

    public void NextTip()
    {
        CurrentTipIndex++;

        if (CurrentTipIndex >= Tips.Length)
        {
            // wrap around
            CurrentTipIndex = 0;
        }

        UpdateCurrentTip();
        RestartTipTimer();
    }


    IEnumerator SwitchTip()
    {
        while (true)
        {
            yield return new WaitForSeconds(7f);

            NextTip();
        }
    }

    IEnumerator FadeText(TMP_Text text, float targetAlpha, float duration)
    {
        Color color = text.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            text.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        text.color = new Color(color.r, color.g, color.b, targetAlpha);
    }

    void RestartTipTimer()
    {
        StopCoroutine(TipTimer);
        TipTimer = StartCoroutine(SwitchTip());
    }
}
