using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public bool connected;
    public bool inGame;
    public bool isHost;
    public ulong myClientId;

    //[SerializeField]
    //private GameObject createMenu, lobbyMenu;

    //public Dictionary<ulong, GameObject> playerInfo = new Dictionary<ulong, GameObject>();

    public static GameManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HostCreated()
    {
        //createMenu.SetActive(false);
        //lobbyMenu.SetActive(true);
        isHost = true;
        connected = true;
    }

    public void ConnectedAsAClient()
    {
        //createMenu.SetActive(false);
        //lobbyMenu.SetActive(true);
        isHost = false;
        connected = true;
    }

    public void Disconnect()
    {
        //playerInfo.clear();

        //createMenu.SetActive(true);
        //lobbyMenu.SetActive(false);
        isHost = false;
        connected = false;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
