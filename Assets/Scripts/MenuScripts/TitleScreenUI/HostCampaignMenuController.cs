using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Steamworks;

public class HostCampaignMenuController : MonoBehaviour
{
    public GameObject HostCampaignMenu;
    public GameObject CampaignMenu;

    public List<TextMeshProUGUI> players = new List<TextMeshProUGUI>();

    

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
