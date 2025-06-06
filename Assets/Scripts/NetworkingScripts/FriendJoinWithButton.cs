using UnityEngine;
using TMPro;
using Steamworks;


public class FriendJoinWithButton : MonoBehaviour
{
    [SerializeField]
    private Friend friend;
    [SerializeField]
    private TextMeshProUGUI friendName;
    [SerializeField]
    private TextMeshProUGUI players;

    public void SetFriend(Friend f)
    {
        friend = f;
        friendName.text = friend.Name;
        players.text = GetPlayers().ToString();
    }

    public void JoinFriendLobby()
    {
        
        if (friend.GameInfo.Value.Lobby.HasValue)
        {
            Debug.Log(friend.GameInfo.Value.Lobby.Value.Id);
            GameNetworkManager.Instance.JoinWithButton(friend.GameInfo.Value.Lobby.Value);
        }
    }

    public void ChangeCanvasMode(int i)
    {
        PanelSwapper.Instance.SwitchPanel(i);
    }

    private int GetPlayers()
    {
        //return friend.GameInfo.Value.Lobby.Value.MemberCount;
        return (int)Random.Range(1, 4);
        //friendGame.Join();
    }
}
