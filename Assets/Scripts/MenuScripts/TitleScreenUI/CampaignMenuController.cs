using UnityEngine;

public class CampaignMenuController : MonoBehaviour
{
    public GameObject CampaignMenu;
    public GameObject HostCampaignMenu;
    public GameObject JoinCampaignMenu;
    public GameObject MainMenu;

    public void HandleHostGameButtonClick()
    {
        SwitchTo(HostCampaignMenu);
        GameNetworkManager.Instance.StartHost(4);
    }

    public void HandleJoinGameButtonClick()
    {
        SwitchTo(JoinCampaignMenu);
    }

    public void HandleBackButtonClick()
    {
        SwitchTo(MainMenu);
    }

    private void SwitchTo(GameObject target)
    {
        CampaignMenu.SetActive(false);
        HostCampaignMenu.SetActive(false);
        JoinCampaignMenu.SetActive(false);
        MainMenu.SetActive(false);

        target.SetActive(true);
    }
}
