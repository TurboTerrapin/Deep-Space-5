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

}
