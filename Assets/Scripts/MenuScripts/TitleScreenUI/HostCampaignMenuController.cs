using UnityEngine;

public class HostCampaignMenuController : MonoBehaviour
{
    public GameObject HostCampaignMenu;
    public GameObject CampaignMenu;

    public void HandleXButtonClick()
    {
        SwitchTo(CampaignMenu);
    }

    public void HandleEngageButtonClick()
    {
        
    }

    private void SwitchTo(GameObject target)
    {
        HostCampaignMenu.SetActive(false);
        CampaignMenu.SetActive(false);

        target.SetActive(true);
    }
}
