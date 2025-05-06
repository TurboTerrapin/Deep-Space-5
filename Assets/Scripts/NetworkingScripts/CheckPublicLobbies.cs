using UnityEngine;
using System.Collections.Generic;
using Steamworks;

using Unity.Netcode;
using Steamworks.Data;
using Netcode.Transports.Facepunch;

public class CheckPublicLobbies : MonoBehaviour
{
    [SerializeField]
    private Lobby[] lobbyList = null;
    [SerializeField]
    private List<GameObject> lobbyObjectList = null;

    [SerializeField]
    private GameObject publicLobbyTemplate = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public async void RefreshPublicLobbies()
    {
        LobbyQuery query = new LobbyQuery();
        query.FilterDistanceFar();
        query.WithSlotsAvailable(1);
        lobbyList = await query.RequestAsync();
        foreach (Lobby lobby in lobbyList)
        {
            GameObject lobbyObject = Instantiate<GameObject>(publicLobbyTemplate, transform);
            lobbyObject.GetComponent<PublicLobbyJoinWithButton>().SetLobby(lobby);
            lobbyObjectList.Add(lobbyObject);
        }
    }


}
