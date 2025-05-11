using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class TSSettingsButtonVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;
    public UIBlock2D SettingsMenu;
    public UIBlock2D MainMenu;

    public Color DefaultColor;
    public Color PressedColor;
    public Color HoveredColor;

    public Color DefaultGradientColor;
    public Color PressedGradientColor;
    public Color HoveredGradientColor;

    internal static void HandleHover(Gesture.OnHover evt, TSSettingsButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, TSSettingsButtonVisuals target)
    {
        target.Background.Color = target.DefaultColor;
        target.Background.Gradient.Color = target.DefaultGradientColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, TSSettingsButtonVisuals target)
    {
        target.Background.Color = target.PressedColor;
        target.Background.Gradient.Color = target.PressedGradientColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, TSSettingsButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleTSSettingsButtonClick(Gesture.OnClick evt, TSSettingsButtonVisuals target)
    {
        //target.SettingsMenu.gameObject.SetActive(true);
        //target.MainMenu.gameObject.SetActive(false);
    }
}
