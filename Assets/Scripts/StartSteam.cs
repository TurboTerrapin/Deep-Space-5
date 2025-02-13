using UnityEngine;
using Steamworks;

public class StartSteam : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try
        {
            Steamworks.SteamClient.Init(480, true);
        }
        catch (System.Exception e)
        {
            // Something went wrong! Steam is closed?
        }

    }


    private void OnDisable()
    {
        Steamworks.SteamClient.Shutdown();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
