using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    public UnityEngine.UI.Button CampaignButton;
    public TMP_Text CampaignText;
    public RawImage CampaignBorder;

    public GameObject TutorialNotif;

    private const string HasBeatenTutorialKey = "HasBeatenTutorial";

    private void Start()
    {
        GameObject LoadHandler = GameObject.Find("LoadHandler");
        if (LoadHandler != null)
        {
            LoadHandler.GetComponent<LoadHandler>().endLoad(false);
        }

        LoadTutorialData();
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

            TutorialNotif.SetActive(false);
        }
        else
        {
            Debug.Log("HasBeatenTutorial: " + HasBeatenTutorial);

            CampaignText.color = new Color(0.494f, 0.494f, 0.494f, 0.8f);

            CampaignBorder.color = new Color(0f, 0.58f, 0.62f, 0.8f);

            TutorialNotif.SetActive(true);
        }
    }

    public void BeatTutorial()
    {
        PlayerPrefs.SetInt(HasBeatenTutorialKey , 0);
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
