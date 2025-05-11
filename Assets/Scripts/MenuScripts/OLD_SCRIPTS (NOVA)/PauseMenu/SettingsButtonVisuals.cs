using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class SettingsButtonVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;

    public Color DefaultColor;
    public Color PressedColor;
    public Color HoveredColor;
    public Color DefaultGradientColor;
    public Color PressedGradientColor;
    public Color HoveredGradientColor;

    internal static void HandleHover(Gesture.OnHover evt, SettingsButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, SettingsButtonVisuals target)
    {
        target.Background.Color = target.DefaultColor;
        target.Background.Gradient.Color = target.DefaultGradientColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, SettingsButtonVisuals target)
    {
        target.Background.Color = target.PressedColor;
        target.Background.Gradient.Color = target.PressedGradientColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, SettingsButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleSettingsButtonClick(Gesture.OnClick evt, SettingsButtonVisuals target)
    {
        Transform pauseMenu = target.Background.transform;
        while (pauseMenu.parent.name != "Main Camera") //will loop through parents until reaches camera
        {
            pauseMenu = pauseMenu.parent;
        }
        pauseMenu.gameObject.SetActive(false);
        pauseMenu.parent.GetChild(2).gameObject.SetActive(true);
    }
}