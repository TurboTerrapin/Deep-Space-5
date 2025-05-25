using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneData
{
    public static string targetUI = null;
}

public class PauseMenuController : MonoBehaviour
{
    public GameObject PauseMenu;
    public GameObject ControlsMenu;
    public GameObject SettingsMenu;

    public void HandleResumeButtonClick()
    {
        PauseMenu.SetActive(false);
    }

    public void HandleControlsButtonClick()
    {
        SwitchTo(ControlsMenu);
    }

    public void HandleSettingsButtonClick()
    {
        SwitchTo(SettingsMenu);
    }

    public void HandleQuitButtonClick()
    {
        SceneData.targetUI = "MainMenu";
        SceneManager.LoadScene("TitleScreen");
    }

    private void SwitchTo(GameObject target)
    {
        PauseMenu.SetActive(false);
        ControlsMenu.SetActive(false);
        SettingsMenu.SetActive(false);

        target.SetActive(true);
    }
}
