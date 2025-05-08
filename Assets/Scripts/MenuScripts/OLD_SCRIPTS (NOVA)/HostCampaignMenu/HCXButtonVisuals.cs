using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class HCXButtonVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;
    public UIBlock2D CampaignMenu;
    public UIBlock2D HostCampaignMenu;

    public Color DefaultColor;
    public Color PressedColor;
    public Color HoveredColor;

    public Color DefaultGradientColor;
    public Color PressedGradientColor;
    public Color HoveredGradientColor;

    internal static void HandleHover(Gesture.OnHover evt, HCXButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, HCXButtonVisuals target)
    {
        target.Background.Color = target.DefaultColor;
        target.Background.Gradient.Color = target.DefaultGradientColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, HCXButtonVisuals target)
    {
        target.Background.Color = target.PressedColor;
        target.Background.Gradient.Color = target.PressedGradientColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, HCXButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleHCXButtonClick(Gesture.OnClick evt, HCXButtonVisuals target)
    {
        target.CampaignMenu.gameObject.SetActive(true);
        target.HostCampaignMenu.gameObject.SetActive(false);
        
    }
}