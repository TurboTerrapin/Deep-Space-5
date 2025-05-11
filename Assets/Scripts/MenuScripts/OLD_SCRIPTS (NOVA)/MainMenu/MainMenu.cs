using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScreen : MonoBehaviour
{
    public UIBlock2D Root = null;
    public UIBlock2D CampaignMenu;
    public UIBlock2D SettingsMenu;

    private void Start()
    {
        CampaignMenu.gameObject.SetActive(false);
        //LogsScreen.gameObject.SetActive(false);
        //SettingsMenu.gameObject.SetActive(false);
        //QuitMenu.gameObject.SetActive(false);

        //CampaignButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, CampaignButtonVisuals>(CampaignButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, CampaignButtonVisuals>(CampaignButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, CampaignButtonVisuals>(CampaignButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, CampaignButtonVisuals>(CampaignButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, CampaignButtonVisuals>(CampaignButtonVisuals.HandleCampaignButtonClick);

        //LogsButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, LogsButtonVisuals>(LogsButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, LogsButtonVisuals>(LogsButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, LogsButtonVisuals>(LogsButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, LogsButtonVisuals>(LogsButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, LogsButtonVisuals>(LogsButtonVisuals.HandleLogsButtonClick);

        //TSSettingsButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, TSSettingsButtonVisuals>(TSSettingsButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, TSSettingsButtonVisuals>(TSSettingsButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, TSSettingsButtonVisuals>(TSSettingsButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, TSSettingsButtonVisuals>(TSSettingsButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, TSSettingsButtonVisuals>(TSSettingsButtonVisuals.HandleTSSettingsButtonClick);

        //TSQuitButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, MMQuitButtonVisuals>(MMQuitButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, MMQuitButtonVisuals>(MMQuitButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, MMQuitButtonVisuals>(MMQuitButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, MMQuitButtonVisuals>(MMQuitButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, MMQuitButtonVisuals>(MMQuitButtonVisuals.HandleMMQuitButtonClick);

        //TSXButtonVisuals
        Root.AddGestureHandler<Gesture.OnHover, TSXButtonVisuals>(TSXButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, TSXButtonVisuals>(TSXButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, TSXButtonVisuals>(TSXButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, TSXButtonVisuals>(TSXButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, TSXButtonVisuals>(TSXButtonVisuals.HandleTSXButtonClick);
    }
}