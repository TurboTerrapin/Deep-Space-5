using UnityEngine;
using System.Collections;

public class TipsController : MonoBehaviour
{
    public GameObject[] Tips;

    private int CurrentTipIndex = 0;

    private Coroutine TipTimer;

    private void Start()
    {
        UpdateCurrentTip();
        TipTimer = StartCoroutine(SwitchTip());
    }

    private void UpdateCurrentTip()
    {
        for (int i = 0; i < Tips.Length; i++)
        {
            Tips[i].SetActive(false);
        }

        Tips[CurrentTipIndex].SetActive(true);

        CanvasGroup canvasGroup = Tips[CurrentTipIndex].GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0f;

        StartCoroutine(FadeTip(canvasGroup, 1f, 1f));

        //Debug.Log(CurrentTipIndex);
    }

    public void PreviousTip()
    {
        CurrentTipIndex--;
        if (CurrentTipIndex < 0)
        {
            // Wrap around
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
            // Wrap around
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

    IEnumerator FadeTip(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            canvasGroup.alpha = a;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void RestartTipTimer()
    {
        StopCoroutine(TipTimer);
        TipTimer = StartCoroutine(SwitchTip());
    }
}