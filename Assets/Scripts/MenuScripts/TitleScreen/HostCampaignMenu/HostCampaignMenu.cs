using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostCampaignScreen : MonoBehaviour
{
    public UIBlock2D Root = null;

    [Header("Temporary")]
    public FriendsList FriendsList = new FriendsList();
    public FriendsListVisuals FriendsListItemView = null;

    private void Start()
    {
        //HCXButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, HCXButtonVisuals>(HCXButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, HCXButtonVisuals>(HCXButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, HCXButtonVisuals>(HCXButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, HCXButtonVisuals>(HCXButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, HCXButtonVisuals>(HCXButtonVisuals.HandleHCXButtonClick);

        ////EngageButtonVisuals
        //Root.AddGestureHandler<Gesture.OnHover, EngageButtonVisuals>(EngageButtonVisuals.HandleHover);
        //Root.AddGestureHandler<Gesture.OnUnhover, EngageButtonVisuals>(EngageButtonVisuals.HandleUnhover);
        //Root.AddGestureHandler<Gesture.OnPress, EngageButtonVisuals>(EngageButtonVisuals.HandlePress);
        //Root.AddGestureHandler<Gesture.OnRelease, EngageButtonVisuals>(EngageButtonVisuals.HandleRelease);
        //Root.AddGestureHandler<Gesture.OnClick, EngageButtonVisuals>(EngageButtonVisuals.HandleEngageButtonClick);

    }
    private void PopulateFriendsList()
    {
        List<FriendItemData> data = new List<FriendItemData>();
        foreach (string name in FriendsList.Options)
        {
            data.Add(new FriendItemData() { Username = name });
        }

        //FriendsListItemView.FriendsListView.OnBind = BindFriendItem;
        //FriendsListItemView.FriendsListView.DataSource = data;
    }

    private void BindFriendItem(Data.OnBind<FriendItemData> evt)
    {
        //FriendItemVisuals visuals = evt.Visuals as FriendItemVisuals;
        //if (visuals != null)
        //{
        //    visuals.Label.Text = evt.UserData.Username;
        //}
    }

    public class FriendItemData
    {
        public string Username;
    }
}