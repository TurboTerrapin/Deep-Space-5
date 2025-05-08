using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Setting
{
    public string Name;
}

//toggle
[System.Serializable]
public class BoolSetting : Setting
{
    public bool State;

    public void ApplySetting()
    {
        Screen.fullScreen = State;
        //PlayerPrefs.SetInt("Fullscreen", State ? 1 : 0);
        //PlayerPrefs.Save();
    }
}

//slider
[System.Serializable]
public class FloatSetting : Setting
{
    [SerializeField]
    private float value;
    public float min;
    public float max;
    public string ValueFormat = "{0:0.0}";

    public float Value
    {
        get => Mathf.Clamp(value, min, max);
        set => this.value = Mathf.Clamp(value, min, max);
    }

    public string DisplayValue => string.Format(ValueFormat, Value);
}

//dropdown
[System.Serializable]
public class MultiOptionSetting : Setting
{
    private const string NothingSelected = "None";

    public string[] Options = new string[0];
    public int SelectedIndex = 0;

    //if the selected index is valid, return the corresponding option. Otherwise, return "None";
    public string CurrentSelection => SelectedIndex >= 0 && SelectedIndex < Options.Length ? Options[SelectedIndex] : NothingSelected;
}

//resolution
[System.Serializable]
public class ResolutionSetting : MultiOptionSetting
{
    public Resolution[] AvailableResolutions;

    public void Initialize()
    {
        AvailableResolutions = Screen.resolutions;
        Options = new string[AvailableResolutions.Length];

        for (int i = 0; i < AvailableResolutions.Length; i++)
        {
            Resolution res = AvailableResolutions[i];
            Options[i] = $"{res.width} x {res.height}";
        }
        for (int i = 0; i < AvailableResolutions.Length; i++)
        {
            if (AvailableResolutions[i].width == Screen.currentResolution.width && AvailableResolutions[i].height == Screen.currentResolution.height)
            {
                SelectedIndex = i;
                break;
            }
        }
    }

    public void ApplySetting()
    {
        if (SelectedIndex >= 0 && SelectedIndex < AvailableResolutions.Length)
        {
            Resolution selectedResolution = AvailableResolutions[SelectedIndex];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
        }
    }

}

// HUDdropdown
[System.Serializable]
public class HUDMultiOptionSetting : Setting
{
    private const string HUDNothingSelected = "None";

    public string[] HUDOptions = new string[0];
    public int HUDSelectedIndex = 0;

    //if the selected index is valid, return the corresponding option. Otherwise, return "None";
    public string HUDCurrentSelection => HUDSelectedIndex >= 0 && HUDSelectedIndex < HUDOptions.Length ? HUDOptions[HUDSelectedIndex] : HUDNothingSelected;

    public int GetSelectedIndex()
    {
        return HUDSelectedIndex;
    }
} 