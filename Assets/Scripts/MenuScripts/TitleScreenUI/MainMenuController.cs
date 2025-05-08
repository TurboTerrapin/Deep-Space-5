using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject CampaignMenu;
    //public GameObject LogsScreen;
    public GameObject SettingsMenu;

    public void HandleCampaignButtonClick()
    {
        SwitchTo(CampaignMenu);
    }

    public void HandleLogsButtonClick()
    {
        //SwitchTo(LogsScreen);
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
        SettingsMenu.SetActive(false);

        target.SetActive(true);
    }
}
