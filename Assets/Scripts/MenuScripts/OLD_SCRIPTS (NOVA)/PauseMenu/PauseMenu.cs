using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public UIBlock Root = null;

    private void Start()
    {
        //Resume Button
        Root.AddGestureHandler<Gesture.OnHover, ResumeButtonVisuals>(ResumeButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, ResumeButtonVisuals>(ResumeButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, ResumeButtonVisuals>(ResumeButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, ResumeButtonVisuals>(ResumeButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, ResumeButtonVisuals>(ResumeButtonVisuals.HandleResumeButtonClick);

        //Controls Button
        Root.AddGestureHandler<Gesture.OnHover, ControlsButtonVisuals>(ControlsButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, ControlsButtonVisuals>(ControlsButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, ControlsButtonVisuals>(ControlsButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, ControlsButtonVisuals>(ControlsButtonVisuals.HandleRelease);

        //Settings Button
        Root.AddGestureHandler<Gesture.OnHover, SettingsButtonVisuals>(SettingsButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, SettingsButtonVisuals>(SettingsButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, SettingsButtonVisuals>(SettingsButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, SettingsButtonVisuals>(SettingsButtonVisuals.HandleRelease);
        Root.AddGestureHandler<Gesture.OnClick, SettingsButtonVisuals>(SettingsButtonVisuals.HandleSettingsButtonClick);

        //Quit Button
        Root.AddGestureHandler<Gesture.OnHover, QuitButtonVisuals>(QuitButtonVisuals.HandleHover);
        Root.AddGestureHandler<Gesture.OnUnhover, QuitButtonVisuals>(QuitButtonVisuals.HandleUnhover);
        Root.AddGestureHandler<Gesture.OnPress, QuitButtonVisuals>(QuitButtonVisuals.HandlePress);
        Root.AddGestureHandler<Gesture.OnRelease, QuitButtonVisuals>(QuitButtonVisuals.HandleRelease);
    }
}