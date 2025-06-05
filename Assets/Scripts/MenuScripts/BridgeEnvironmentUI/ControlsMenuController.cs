using UnityEngine;

public class ControlsMenuController : MonoBehaviour
{
    public GameObject ControlsMenu;
    public GameObject PauseMenu;

    public void HandleXButtonClick()
    {
        SwitchTo(PauseMenu);
    }

    private void SwitchTo(GameObject target)
    {
        PauseMenu.SetActive(false);
        ControlsMenu.SetActive(false);

        target.SetActive(true);
    }
}

