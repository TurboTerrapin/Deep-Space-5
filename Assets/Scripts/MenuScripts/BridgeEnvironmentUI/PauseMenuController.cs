using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject PauseMenu;
    //public GameObject ControlsMenu;
    public GameObject SettingsMenu;

    public void HandleResumeButtonClick()
    {
        PauseMenu.SetActive(false);
    }

    public void HandleControlsButtonClick()
    {
        //SwitchTo(ControlsMenu);
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
        PauseMenu.SetActive(false);
        //ControlsMenu.SetActive(false);
        SettingsMenu.SetActive(false);

        target.SetActive(true);
    }
}
