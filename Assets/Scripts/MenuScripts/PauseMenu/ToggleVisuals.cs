using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class ToggleVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D CheckBox = null;
    public UIBlock2D CheckMark = null;

    public Color DefaultColor;
    public Color HoveredColor;
    public Color PressedColor;

    public bool IsChecked
    {
        get => CheckMark.gameObject.activeSelf;
        set => CheckMark.gameObject.SetActive(value);
    }

    internal static void HandleHover(Gesture.OnHover evt, ToggleVisuals target)
    {
        target.CheckMark.Color = target.HoveredColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, ToggleVisuals target)
    {
        target.CheckMark.Color = target.PressedColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, ToggleVisuals target)
    {
        target.CheckMark.Color = target.HoveredColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, ToggleVisuals target)
    {
        target.CheckMark.Color = target.DefaultColor;
    }

}
