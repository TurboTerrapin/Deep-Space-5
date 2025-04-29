using Nova;
using UnityEngine;

[System.Serializable]
public class FriendItemVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;
    public UIBlock2D InviteButton = null;
}

[System.Serializable]
public class FriendsListVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;
    public UIBlock ExpandedRoot = null;
    public ListView FriendsListView = null;

    public Color DefaultColor;
    public Color HoveredColor;
    public Color PressedColor;
    public Color PrimaryRowColor;
    public Color SecondaryRowColor;
}

