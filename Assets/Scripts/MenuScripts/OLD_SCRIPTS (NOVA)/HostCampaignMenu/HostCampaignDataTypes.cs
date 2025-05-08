using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nova;

[System.Serializable]
public abstract class HostCampaignDataTypes
{
    public string Name;
}

[System.Serializable]
public class FriendsList : HostCampaignDataTypes
{
    public string[] Options = new string[]
    {
        "Bmusial",
        "TurboTerrapin"
    };
}
