using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropDownItemVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;
}

[System.Serializable]
public class DropDownVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public TextBlock SelectedLabel = null;
    public UIBlock2D Background = null;
    public UIBlock ExpandedRoot = null;
    public ListView OptionsList = null;

    public Color DefaultColor;
    public Color HoveredColor;
    public Color PressedColor;
    public Color PrimaryRowColor;
    public Color SecondaryRowColor;

    private MultiOptionSetting dataSource = null;
    private bool eventHandlerRegistered = false;

    public bool IsExpanded => ExpandedRoot.gameObject.activeSelf;

    public void Expand(MultiOptionSetting dataSource)
    {
        this.dataSource = dataSource;

        EnsureEventHandler();

        ExpandedRoot.gameObject.SetActive(true);
        OptionsList.SetDataSource(dataSource.Options);
        //scrollbar position
        OptionsList.JumpToIndex(dataSource.SelectedIndex);

    }

    private void EnsureEventHandler()
    {
        if (eventHandlerRegistered)
        {
            return;
        }

        eventHandlerRegistered = true;

        OptionsList.AddGestureHandler<Gesture.OnHover, DropDownItemVisuals>(HandleItemHovered);
        OptionsList.AddGestureHandler<Gesture.OnUnhover, DropDownItemVisuals>(HandleItemUnhovered);
        OptionsList.AddGestureHandler<Gesture.OnPress, DropDownItemVisuals>(HandleItemPressed);
        OptionsList.AddGestureHandler<Gesture.OnRelease, DropDownItemVisuals>(HandleItemReleased);
        OptionsList.AddGestureHandler<Gesture.OnClick, DropDownItemVisuals>(HandleItemClicked);

        OptionsList.AddDataBinder<string, DropDownItemVisuals>(BindItem);
    }

    private void HandleItemHovered(Gesture.OnHover evt, DropDownItemVisuals target, int index)
    {
        target.Background.Color = HoveredColor;
    }

    private void HandleItemPressed(Gesture.OnPress evt, DropDownItemVisuals target, int index)
    {
        target.Background.Color = PressedColor;
    }

    private void HandleItemUnhovered(Gesture.OnUnhover evt, DropDownItemVisuals target, int index)
    {
        target.Background.Color = index % 2 == 0 ? PrimaryRowColor : SecondaryRowColor;
    }
   
    private void HandleItemReleased(Gesture.OnRelease evt, DropDownItemVisuals target, int index)
    {
        target.Background.Color = HoveredColor;
    }

    private void HandleItemClicked(Gesture.OnClick evt, DropDownItemVisuals target, int index)
    {
        dataSource.SelectedIndex = index;
        SelectedLabel.Text = dataSource.CurrentSelection;

        if (dataSource is ResolutionSetting resolutionSetting)
        {
            resolutionSetting.ApplySetting();
        }

        evt.Consume();
        Collapse();
    }

    private void BindItem(Data.OnBind<string> evt, DropDownItemVisuals target, int index)
    {
        target.Label.Text = evt.UserData;
        target.Background.Color = index % 2 == 0 ? PrimaryRowColor : SecondaryRowColor;
    }

    internal static void HandleHover(Gesture.OnHover evt, DropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.HoveredColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, DropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.PressedColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, DropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.HoveredColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, DropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.DefaultColor;
    }

    public void Collapse()
    {
        ExpandedRoot.gameObject.SetActive(false);
    }
}
