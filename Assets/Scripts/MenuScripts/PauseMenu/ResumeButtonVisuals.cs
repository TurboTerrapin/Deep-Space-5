using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class ResumeButtonVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;

    public Color DefaultColor;
    public Color PressedColor;
    public Color HoveredColor;

    public Color DefaultGradientColor;
    public Color PressedGradientColor;
    public Color HoveredGradientColor;

    internal static void HandleHover(Gesture.OnHover evt, ResumeButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, ResumeButtonVisuals target)
    {
        target.Background.Color = target.DefaultColor;
        target.Background.Gradient.Color = target.DefaultGradientColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, ResumeButtonVisuals target)
    {
        target.Background.Color = target.PressedColor;
        target.Background.Gradient.Color = target.PressedGradientColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, ResumeButtonVisuals target)
    {
        target.Background.Color = target.HoveredColor;
        target.Background.Gradient.Color = target.HoveredGradientColor;
    }

    internal static void HandleResumeButtonClick(Gesture.OnClick evt, ResumeButtonVisuals target)
    {
        /*
        Transform player = target.Background.transform;

        while (player.name != "Player")
        {
            player = player.parent;
        }

        */
        Transform thisPlayer = null;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Debug.Log(players.Length + " " + player.GetComponent<PlayerMove>().IsOwner);
            if (player.GetComponent<PlayerMove>().IsOwner)
            {
                thisPlayer = target.Background.transform;
            }
        }
        if (thisPlayer == null)
        {
            Debug.Log("Shit not working");
            return;
        }
        //ControlScript control_script = thisPlayer.GetComponent<ControlScript>();
        ControlScript.Instance.unpause();
    }

}