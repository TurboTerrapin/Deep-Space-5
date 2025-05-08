// Settings Controller for the TitleScreen Scene

using UnityEngine;

public class TSSettingsMenuController : MonoBehaviour
{
    public GameObject SettingsMenu;
    public GameObject MainMenu;

    public void HandleXButtonClick()
    {
        SettingsMenu.SetActive(false);
        MainMenu.SetActive(true);
    }
}
