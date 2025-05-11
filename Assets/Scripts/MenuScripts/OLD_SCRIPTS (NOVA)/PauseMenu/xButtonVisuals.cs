using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class XButtonVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;

    public Color DefaultColor;
    public Color PressedColor;
    public Color HoveredColor;

    public Color DefaultGradientColor;
    public Color PressedGradientColor;
    public Color HoveredGradientColor;

    internal static void HandleHover(Gesture.OnHover evt, XButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, XButtonVisuals target)
    {
        target.Background.Color = target.DefaultColor;
        target.Background.Gradient.Color = target.DefaultGradientColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, XButtonVisuals target)
    {
        target.Background.Color = target.PressedColor;
        target.Background.Gradient.Color = target.PressedGradientColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, XButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleXButtonClick(Gesture.OnClick evt, XButtonVisuals target)
    {
        Transform settingsMenu = target.Background.transform;

        while (settingsMenu.parent.name != "Main Camera") //will loop through parents until reaches camera
        { 
            settingsMenu = settingsMenu.parent;
        }

        settingsMenu.gameObject.SetActive(false);
        settingsMenu.parent.GetChild(1).gameObject.SetActive(true);
    }

}