using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignMenuScreen : MonoBehaviour
{
    public UIBlock2D Root = null;

    private void Start()
    {
        //HostButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, HostButtonVisuals>(HostButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, HostButtonVisuals>(HostButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, HostButtonVisuals>(HostButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, HostButtonVisuals>(HostButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, HostButtonVisuals>(HostButtonVisuals.HandleHostButtonClick);

        //JoinButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, JoinButtonVisuals>(JoinButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, JoinButtonVisuals>(JoinButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, JoinButtonVisuals>(JoinButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, JoinButtonVisuals>(JoinButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, JoinButtonVisuals>(JoinButtonVisuals.HandleJoinButtonClick);

        //BackButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, BackButtonVisuals>(BackButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, BackButtonVisuals>(BackButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, BackButtonVisuals>(BackButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, BackButtonVisuals>(BackButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, BackButtonVisuals>(BackButtonVisuals.HandleBackButtonClick);
    }
}