using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public UIBlock Root = null;

    [Header("Temporary")]
    //toggle
    public BoolSetting BoolSetting = new BoolSetting();
    public ItemView ToggleItemView = null;
    //volume slider
    public FloatSetting FloatSetting = new FloatSetting();
    public ItemView SliderItemView = null;
    //dropdown
    public MultiOptionSetting MultiOptionSetting = new MultiOptionSetting();
    public ItemView DropDownItemView = null;
    //resolution
    public ResolutionSetting ResolutionSetting = new ResolutionSetting();
    public ItemView ResolutionDropDownItemView = null;
    //HUD
    public HUDMultiOptionSetting HUDMultiOptionSetting = new HUDMultiOptionSetting();
    public ItemView HUDDropDownItemView = null;

    private void Start()
    {
        ResolutionSetting.Initialize();

        //Toggle
        Root.AddGestureHandler<Gesture.OnHover, ToggleVisuals>(ToggleVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, ToggleVisuals>(ToggleVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, ToggleVisuals>(ToggleVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, ToggleVisuals>(ToggleVisuals.HandleRelease);

        //Resolution
        Root.AddGestureHandler<Gesture.OnHover, DropDownVisuals>(DropDownVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, DropDownVisuals>(DropDownVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, DropDownVisuals>(DropDownVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, DropDownVisuals>(DropDownVisuals.HandleRelease);

        //X Button
        Root.AddGestureHandler<Gesture.OnHover, XButtonVisuals>(XButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, XButtonVisuals>(XButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, XButtonVisuals>(XButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, XButtonVisuals>(XButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, XButtonVisuals>(XButtonVisuals.HandleXButtonClick);

        //HUD
        Root.AddGestureHandler<Gesture.OnHover, HUDDropDownVisuals>(HUDDropDownVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, HUDDropDownVisuals>(HUDDropDownVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, HUDDropDownVisuals>(HUDDropDownVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, HUDDropDownVisuals>(HUDDropDownVisuals.HandleRelease);

        //will change state
        Root.AddGestureHandler<Gesture.OnClick, ToggleVisuals>(HandleToggleClicked);
        Root.AddGestureHandler<Gesture.OnDrag, SliderVisuals>(HandleSliderDragged);
        Root.AddGestureHandler<Gesture.OnClick, DropDownVisuals>(HandleDropDownClicked);
        Root.AddGestureHandler<Gesture.OnClick, HUDDropDownVisuals>(HandleHUDDropDownClicked);

        //Initialize UI elements
        BindToggle(BoolSetting, ToggleItemView.Visuals as ToggleVisuals);
        BindSlider(FloatSetting, SliderItemView.Visuals as SliderVisuals);
        BindDropDown(MultiOptionSetting, DropDownItemView.Visuals as DropDownVisuals);
        BindDropDown(ResolutionSetting, ResolutionDropDownItemView?.Visuals as DropDownVisuals); //resolution
        BindHUDDropDown(HUDMultiOptionSetting, HUDDropDownItemView.Visuals as HUDDropDownVisuals); //hud
    }

    private void HandleToggleClicked(Gesture.OnClick evt, ToggleVisuals target)
    {
        BoolSetting.State = !BoolSetting.State;
        target.IsChecked = BoolSetting.State;

        BoolSetting.ApplySetting();
    }

    private void HandleSliderDragged(Gesture.OnDrag evt, SliderVisuals target)
    {
        Vector3 currentPointerPos = evt.PointerPositions.Current;

        float localXPos = target.SliderBackground.transform.InverseTransformPoint(currentPointerPos).x;
        float sliderWidth = target.SliderBackground.CalculatedSize.X.Value;

        float distanceFromLeft = localXPos + .5f * sliderWidth;
        float percentFromLeft = Mathf.Clamp01(distanceFromLeft / sliderWidth);

        FloatSetting.Value = FloatSetting.min + percentFromLeft * (FloatSetting.max - FloatSetting.min);

        target.FillBar.Size.X.Percent = percentFromLeft;
        target.ValueLabel.Text = FloatSetting.DisplayValue;

        FindAnyObjectByType<AudioManager>()?.SetMasterVolume(percentFromLeft);
    }

    private void HandleDropDownClicked(Gesture.OnClick evt, DropDownVisuals target)
    {
        if(target.IsExpanded)
        {
            target.Collapse();
        }
        else
        {
            if (target == ResolutionDropDownItemView?.Visuals)
            {
                target.Expand(ResolutionSetting);
            }
            else
            {
                target.Expand(MultiOptionSetting);
            }
        }
    }

    private void HandleHUDDropDownClicked(Gesture.OnClick evt, HUDDropDownVisuals target)
    {
        if (target.IsExpanded)
        {
            target.HUDCollapse();

            int selectedIndex = HUDMultiOptionSetting.GetSelectedIndex();
        }
        else
        {
            target.Expand(HUDMultiOptionSetting);
        }
    }

    private void BindToggle(BoolSetting boolSetting, ToggleVisuals visuals)
    {
        boolSetting.State = Screen.fullScreen;
        visuals.Label.Text = boolSetting.Name;
        visuals.IsChecked = boolSetting.State;
    }

    private void BindSlider(FloatSetting setting, SliderVisuals visuals)
    {
        visuals.Label.Text = setting.Name;
        visuals.ValueLabel.Text = setting.DisplayValue;
        visuals.FillBar.Size.X.Percent = (setting.Value - setting.min) / (setting.max - setting.min);
    }

    private void BindDropDown(MultiOptionSetting setting, DropDownVisuals visuals)
    {
        if (visuals == null) return;
        visuals.Label.Text = setting.Name;
        visuals.SelectedLabel.Text = setting.CurrentSelection;
        visuals.Collapse();
    }

    private void BindHUDDropDown(HUDMultiOptionSetting setting, HUDDropDownVisuals visuals)
    {
        if (visuals == null) return;
        visuals.Label.Text = setting.Name;
        visuals.SelectedLabel.Text = setting.HUDCurrentSelection;
        visuals.HUDCollapse();
    }

}
