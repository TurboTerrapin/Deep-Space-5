using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;

public class CheckFriendsList : MonoBehaviour
{

    [SerializeField]
    private GameObject friendUITemplate = null;

    [SerializeField]
    private List<Friend> friendsInGame = new List<Friend>();
    [SerializeField]
    private List<GameObject> friendObjects = new List<GameObject>();


    public float timer = 2;
    public bool lookingForLobbies = true;

    public static CheckFriendsList Instance { get; private set; } = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (lookingForLobbies)
        {
            friendsInGame.Clear();
            timer += Time.deltaTime;
            //Every two seconds
            if (timer > 2)
            {
                timer = 0;
                GetFriendsInGame();
                CreateFriendObjects();
            }
        }
        */

        


        //Just displays the names of those ingame
        if (Input.GetKeyDown(KeyCode.N))
        {
            foreach (Friend friend in friendsInGame)
            {
                Debug.Log(friend.Name + "\n");
            }
        }
    }

    public List<Friend> GetFriendsInGameList()
    {
        return friendsInGame;
    }


    public void RefreshFriendLobbies()
    {
        foreach (GameObject friend in friendObjects)
        {
            Destroy(friend.gameObject);
        }
        friendsInGame.Clear();
        friendObjects.Clear();
        GetFriendsInGame();
        CreateFriendObjects();
    }

    private void GetFriendsInGame()
    {
        //Check all your friends in your list
        foreach (Friend friend in SteamFriends.GetFriends())
        {
            //Friend.FriendGameInfo info = friend.GameInfo.Value;

            //If they're playing the game
            if (friend.IsPlayingThisGame)
            {
                //Add to the list
                friendsInGame.Add(friend);
            }
        }
    }

    private void CreateFriendObjects()
    {
        foreach (Friend friend in friendsInGame)
        {
            //Friend.FriendGameInfo info = friend.GameInfo.Value;
            //if (info.Lobby.HasValue)
            //{
                GameObject friendObject = Instantiate<GameObject>(friendUITemplate, transform);
                friendObject.GetComponent<FriendJoinWithButton>().SetFriend(friend);
                friendObjects.Add(friendObject.gameObject);
            //}

        }
    }

    public void JoinFriendsGame(Friend friend)
    {
        if (friend.GameInfo.Value.Lobby.HasValue)
        {
            friend.GameInfo.Value.Lobby.Value.Join();
            //friendGame.Join();
        }
    }

}
