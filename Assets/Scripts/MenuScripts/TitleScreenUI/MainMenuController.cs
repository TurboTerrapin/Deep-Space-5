using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    //CLASS CONSTANTS
    private static bool CHECK_FOR_TUTORIAL = false;
    private static string HAS_BEATEN_TUTORIAL_KEY = "HasBeatenTutorial";

    public GameObject MainMenu;
    public GameObject CampaignMenu;
    public GameObject LogsMenu;
    public GameObject CustomizationMenu;
    public GameObject SettingsMenu;
    public CanvasGroup MainMenuContents;

    public UnityEngine.UI.Button CampaignButton;
    public TMP_Text CampaignText;
    public RawImage CampaignBorder;

    public GameObject CompleteCampaignNotif;

    private void Start()
    {
        GameObject LoadHandler = GameObject.FindGameObjectWithTag("LoadHandler");
        if (LoadHandler != null)
        {
            LoadHandler.GetComponent<LoadHandler>().endLoad(false);
        }

        LoadTutorialData();

        MainMenuContents.alpha = 0f;
        StartCoroutine(FadeTo(MainMenuContents, 1f, 1f));
    }

    public void LoadTutorialData()
    {
        // Get true or false
        bool HasBeatenTutorial = (PlayerPrefs.GetInt(HAS_BEATEN_TUTORIAL_KEY, 0) == 1);

        // Make uninteractble based on true or not
        CampaignButton.interactable = (HasBeatenTutorial || !CHECK_FOR_TUTORIAL);

        if (HasBeatenTutorial || !CHECK_FOR_TUTORIAL)
        {
            CompleteCampaignNotif.SetActive(false);
        }
        else
        {
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

    public static void OnIntroCompleted()
    {
        PlayerPrefs.SetInt(HAS_BEATEN_TUTORIAL_KEY, 1);
        PlayerPrefs.Save();
    }

    public static bool GetIntroCompleted()
    {
        return (PlayerPrefs.GetInt(HAS_BEATEN_TUTORIAL_KEY, 0) == 1);
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

    public void HandleIntroButtonClick()
    {
        GameObject.FindGameObjectWithTag("LoadHandler").GetComponent<LoadHandler>().startLoad();
        SceneManager.LoadScene("IntroSequence");
    }

    public void HandleTrainingButtonClick()
    {
        GameObject.FindGameObjectWithTag("LoadHandler").GetComponent<LoadHandler>().startLoad();
        SceneManager.LoadScene("TrainingEnvironment");
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