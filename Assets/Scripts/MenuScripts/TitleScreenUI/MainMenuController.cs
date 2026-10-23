using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Net.Http.Headers;

public class MainMenuController : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject CampaignMenu;
    public GameObject LogsMenu;
    public GameObject CustomizationMenu;
    public GameObject SettingsMenu;
    public CanvasGroup MainMenuContents;
    public GameObject BrightnessOverlay;

    public UnityEngine.UI.Button CampaignButton;
    public TMP_Text CampaignText;
    public RawImage CampaignBorder;

    public GameObject CompleteCampaignNotif;

    private const string HasBeatenTutorialKey = "HasBeatenTutorial";

    private void Start()
    {
        GameObject LoadHandler = GameObject.Find("LoadHandler");
        if (LoadHandler != null)
        {
            LoadHandler.GetComponent<LoadHandler>().endLoad(false);
        }

        BeatTutorial();
        LoadTutorialData();

        BrightnessOverlay.SetActive(true);

        MainMenuContents.alpha = 0f;
        StartCoroutine(FadeTo(MainMenuContents, 1f, 1f));

    }

    public void LoadTutorialData()
    {
        // Get true or false
        bool HasBeatenTutorial = PlayerPrefs.GetInt(HasBeatenTutorialKey,0) == 1;

        // Make uninteractble based on true or not.
        CampaignButton.interactable = HasBeatenTutorial;

        if (HasBeatenTutorial)
        {
            Debug.Log("HasBeatenTutorial: " + HasBeatenTutorial);

            CompleteCampaignNotif.SetActive(false);
        }
        else
        {
            Debug.Log("HasBeatenTutorial: " + HasBeatenTutorial);

            CampaignText.color = new Color(0.494f, 0.494f, 0.494f, 0.8f);

            CampaignBorder.color = new Color(0f, 0.58f, 0.62f, 0.8f);

            CompleteCampaignNotif.SetActive(true);
        }
    }

    IEnumerator FadeTo(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void BeatTutorial()
    {
        PlayerPrefs.SetInt(HasBeatenTutorialKey , 1);
        PlayerPrefs.Save();
        
    }

    public void HandleCampaignButtonClick()
    {
        SwitchTo(CampaignMenu);
    }

    public void HandleLogsButtonClick()
    {
        SwitchTo(LogsMenu);
    }

    public void HandleCustomizeButtonClick()
    {
        SwitchTo(CustomizationMenu);
    }

    public void HandleSettingsButtonClick()
    {
        SwitchTo(SettingsMenu);
    }

    public void HandleQuitButtonClick()
    {
        Application.Quit();
    }

    private void SwitchTo(GameObject target)
    {
        MainMenu.SetActive(false);
        CampaignMenu.SetActive(false);
        //LogsScreen.SetActive(false);
        CustomizationMenu.SetActive(false);
        SettingsMenu.SetActive(false);

        target.SetActive(true);
    }
}
