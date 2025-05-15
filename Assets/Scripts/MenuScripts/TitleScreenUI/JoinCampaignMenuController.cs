using UnityEngine;

public class JoinCampaignMenuController : MonoBehaviour
{
    public GameObject JoinCampaignMenu;
    public GameObject CampaignMenu;

    public void HandleXButtonClick()
    {
        SwitchTo(CampaignMenu);
    }

    private void SwitchTo(GameObject target)
    {
        JoinCampaignMenu.SetActive(false);
        CampaignMenu.SetActive(false);

        target.SetActive(true);
    }
}
