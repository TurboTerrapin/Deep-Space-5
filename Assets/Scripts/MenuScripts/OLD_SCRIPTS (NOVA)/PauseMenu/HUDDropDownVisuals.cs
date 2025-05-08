using Nova;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HUDDropDownItemVisuals : ItemVisuals
{
    public TextBlock Label = null;
    public UIBlock2D Background = null;
    //public UIBlock2D SelectedIndicator = null;
}

[System.Serializable]
public class HUDDropDownVisuals : ItemVisuals
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

    private HUDMultiOptionSetting dataSource = null;
    private bool eventHandlerRegistered = false;

    public bool IsExpanded => ExpandedRoot.gameObject.activeSelf;

    public GameObject control_script_holder;

    public void Expand(HUDMultiOptionSetting dataSource)
    {
        //Debug.Log("Expand() called");
        
        this.dataSource = dataSource;

        EnsureEventHandler();

        ExpandedRoot.gameObject.SetActive(true);
        OptionsList.SetDataSource(dataSource.HUDOptions);
        //scrollbar position
        OptionsList.JumpToIndex(dataSource.HUDSelectedIndex);

    }

    private void EnsureEventHandler()
    {
        if (eventHandlerRegistered)
        {
            return;
        }

        eventHandlerRegistered = true;

        OptionsList.AddGestureHandler<Gesture.OnHover, HUDDropDownItemVisuals>(HandleItemHovered);
        OptionsList.AddGestureHandler<Gesture.OnUnhover, HUDDropDownItemVisuals>(HandleItemUnhovered);
        OptionsList.AddGestureHandler<Gesture.OnPress, HUDDropDownItemVisuals>(HandleItemPressed);
        OptionsList.AddGestureHandler<Gesture.OnRelease, HUDDropDownItemVisuals>(HandleItemReleased);
        OptionsList.AddGestureHandler<Gesture.OnClick, HUDDropDownItemVisuals>(HandleItemClicked);

        OptionsList.AddDataBinder<string, HUDDropDownItemVisuals>(BindItem);
    }

    private void HandleItemHovered(Gesture.OnHover evt, HUDDropDownItemVisuals target, int index)
    {
        target.Background.Color = HoveredColor;
    }

    private void HandleItemPressed(Gesture.OnPress evt, HUDDropDownItemVisuals target, int index)
    {
        target.Background.Color = PressedColor;
    }

    private void HandleItemUnhovered(Gesture.OnUnhover evt, HUDDropDownItemVisuals target, int index)
    {
        target.Background.Color = index % 2 == 0 ? PrimaryRowColor : SecondaryRowColor;
    }

    private void HandleItemReleased(Gesture.OnRelease evt, HUDDropDownItemVisuals target, int index)
    {
        target.Background.Color = HoveredColor;
    }

    private void HandleItemClicked(Gesture.OnClick evt, HUDDropDownItemVisuals target, int index)
    {
        Debug.Log($"Item clicked: {target.Label.Text}, Index: {index}");
      
        dataSource.HUDSelectedIndex = index;
        Debug.Log($"Setting HUDSelectedIndex to {dataSource.HUDSelectedIndex}");
        ControlScript x = (ControlScript)control_script_holder.GetComponent("ControlScript");
        x.setHUD(dataSource.HUDSelectedIndex);

        SelectedLabel.Text = dataSource.HUDCurrentSelection;
        evt.Consume();

        HUDCollapse();
    }

    private void BindItem(Data.OnBind<string> evt, HUDDropDownItemVisuals target, int index)
    {
        if (target == null)
        {
            Debug.LogError("BindItem error: target is null.");
            return;
        }
        if (target.Label == null)
        {
            Debug.LogError("BindItem error: target.Label is null.");
            return;
        }
        //if (target.SelectedIndicator == null)
        //{
        //    Debug.LogError("BindItem error: target.SelectedIndicator is null.");
        //    return;
        //}
        if (target.Background == null)
        {
            Debug.LogError("BindItem error: target.Background is null.");
            return;
        }

        target.Label.Text = evt.UserData;
        //target.SelectedIndicator.gameObject.SetActive(index == dataSource.HUDSelectedIndex);
        target.Background.Color = index % 2 == 0 ? PrimaryRowColor : SecondaryRowColor;
    }

    internal static void HandleHover(Gesture.OnHover evt, HUDDropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.HoveredColor;
    }

    internal static void HandlePress(Gesture.OnPress evt, HUDDropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.PressedColor;
    }

    internal static void HandleRelease(Gesture.OnRelease evt, HUDDropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.HoveredColor;
    }

    internal static void HandleUnhover(Gesture.OnUnhover evt, HUDDropDownVisuals target)
    {
        if (evt.Receiver.transform.IsChildOf(target.ExpandedRoot.transform))
        {
            return;
        }
        target.Background.Color = target.DefaultColor;
    }

    public void HUDCollapse()
    {
        //Debug.Log("HUDCollapse() called");
        ExpandedRoot.gameObject.SetActive(false);
    }

}