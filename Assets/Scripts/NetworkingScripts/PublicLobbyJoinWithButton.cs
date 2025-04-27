using UnityEngine;
using Steamworks;
using TMPro;
using Unity.Netcode;
using Steamworks.Data;
using Netcode.Transports.Facepunch;


public class PublicLobbyJoinWithButton : MonoBehaviour
{
    [SerializeField]
    private Lobby lobby;

    [SerializeField]
    private TextMeshProUGUI playerText = null;



    public void SetLobby(Lobby l)
    {
        lobby = l;
        playerText.text = l.MemberCount.ToString();
    }

    public void JoinPublicLobby()
    {
        GameNetworkManager.Instance.JoinWithButton(lobby);
    }

}
