using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class TSXButtonVisuals : ItemVisuals
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

    internal static void HandleHover(Gesture.OnHover evt, TSXButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, TSXButtonVisuals target)
    {
        target.Background.Color = target.DefaultColor;
        target.Background.Gradient.Color = target.DefaultGradientColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, TSXButtonVisuals target)
    {
        target.Background.Color = target.PressedColor;
        target.Background.Gradient.Color = target.PressedGradientColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, TSXButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleTSXButtonClick(Gesture.OnClick evt, TSXButtonVisuals target)
    {
        target.SettingsMenu.gameObject.SetActive(false);
        target.MainMenu.gameObject.SetActive(true);
    }
}