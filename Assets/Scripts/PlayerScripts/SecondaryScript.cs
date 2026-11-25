/*
    SecondaryScript.cs
    - Helps with secondary info that isn't primary control interactions
    - Handles displaying pop-up notifications
    - Handles displaying pop-up dialogue
    Contributor(s): Jake Schott
    Last Updated: 11/20/2026
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SecondaryScript : MonoBehaviour
{
    //CLASS CONSTANTS
    public static Color DEFAULT_BORDER_CORDER = new Color(0.12f, 0.12f, 0.12f, 1.0f);

    public GameObject secondary_info;

    private GameObject permanent_overlay;
    private GameObject stations_button;
    private GameObject current_station_indicator;
    private GameObject station_functions;
    private GameObject sitting_overlay;
    private GameObject sitting_left_side;
    private GameObject sitting_right_side;
    private GameObject dialogue_overlay;
    private GameObject primary_default_power_circles;

    private bool permanent_overlay_top_accessible = true;
    private List<int> popup_notification_queue = new List<int>();
    private List<DialogueInfo> popup_dialogue_queue = new List<DialogueInfo>();
    private float displayed_power = 0.0f;
    private Coroutine popup_animation_coroutine = null;
    private Coroutine dialogue_display_coroutine = null;

    private struct DialogueInfo
    {
        public string dialogue_speaker;
        public string dialogue_content;
        public Color dialogue_color;
        public float display_time;

        public DialogueInfo(string speaker, string content, Color c, float time)
        {
            dialogue_speaker = speaker;
            dialogue_content = content;
            dialogue_color = c;
            display_time = time;    
        }
    }

    private void Awake()
    {
        permanent_overlay = secondary_info.transform.GetChild(0).gameObject;
        stations_button = permanent_overlay.transform.GetChild(0).gameObject;
        current_station_indicator = permanent_overlay.transform.GetChild(1).gameObject;
        station_functions = permanent_overlay.transform.GetChild(2).gameObject;
        sitting_overlay = secondary_info.transform.GetChild(1).gameObject;
        sitting_left_side = sitting_overlay.transform.GetChild(0).gameObject;
        sitting_right_side = sitting_overlay.transform.GetChild(1).gameObject;
        dialogue_overlay = secondary_info.transform.GetChild(3).gameObject;
        primary_default_power_circles = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(3).GetChild(1).gameObject;
    }

    public void setPermanentOverlayTopAccessible(bool accessible)
    {
        permanent_overlay_top_accessible = accessible;
        setPermanentOverlayTopVisibility(stations_button.activeSelf && accessible);
    }

    public void setSecondaryInfoVisibility(bool visible)
    {
        if (visible == false)
        {
            clearPopupNotifications();
        }
        secondary_info.SetActive(visible);
    }

    public void updateSecondaryInfoConfiguration(int HUD_setting)
    {
        setPermanentOverlayVisibility(HUD_setting == 0);
        setPermanentOverlayTopVisibility(HUD_setting == 0 && permanent_overlay_top_accessible);
    }

    public void setPermanentOverlayVisibility(bool visible)
    {
        permanent_overlay.SetActive(visible);
    }

    private void setPermanentOverlayTopVisibility(bool visible)
    {
        stations_button.SetActive(visible);
        current_station_indicator.SetActive(visible);
    }

    public void setSittingOverlayVisibility(bool visible)
    {
        sitting_overlay.SetActive(visible);
    }

    public void setSittingRightSideVisibility(bool visible)
    {
        sitting_right_side.SetActive(visible);
    }

    //updates station indicator in top right as well as colors on top
    public void onStationChange(Color c, Texture icon)
    {
        //handle station functions top left indicator and station icon top right indicator
        if (permanent_overlay_top_accessible == true)
        {
            //update station icon
            current_station_indicator.transform.GetChild(2).gameObject.SetActive(icon != null);
            current_station_indicator.transform.GetChild(3).gameObject.SetActive(icon == null);
            current_station_indicator.transform.GetChild(2).GetComponent<UnityEngine.UI.RawImage>().texture = icon;
            current_station_indicator.transform.GetChild(2).GetComponent<UnityEngine.UI.RawImage>().color = c;
            foreach (Transform t in current_station_indicator.transform.GetChild(1))
            {
                t.GetComponent<UnityEngine.UI.RawImage>().color = c;
            }

            //update station buttons
            foreach (Transform t in stations_button.transform.GetChild(1))
            {
                t.GetComponent<UnityEngine.UI.RawImage>().color = c;
            }
        }

        //stop updating if default because default will never be used for sitting overlay
        if (c == DEFAULT_BORDER_CORDER)
        {
            return;
        }

        //update sitting overlay if sitting
        foreach (Transform t in transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1))
        {
            foreach (Transform b in t)
            {
                if (b.GetComponent<UnityEngine.UI.Image>() != null)
                {
                    b.GetComponent<UnityEngine.UI.Image>().color = c;
                }
                else
                {
                    foreach (Transform l in b)
                    {
                        l.GetComponent<UnityEngine.UI.Image>().color = c;
                    }
                }
            }
        }
        foreach (Transform t in transform.GetChild(1).GetChild(0).GetChild(1).GetChild(2).GetChild(1))
        {
            t.GetComponent<UnityEngine.UI.Image>().color = c;
        }
        foreach (Transform t in sitting_left_side.transform)
        {
            foreach (Transform b in t.GetChild(1))
            {
                b.GetComponent<UnityEngine.UI.RawImage>().color = c;
            }
        }
        foreach (Transform t in sitting_right_side.transform)
        {
            foreach (Transform b in t.GetChild(1))
            {
                if (b.GetComponent<UnityEngine.UI.RawImage>() != null)
                {
                    b.GetComponent<UnityEngine.UI.RawImage>().color = c;
                }
                else
                {
                    foreach (Transform l in b)
                    {
                        l.GetComponent<UnityEngine.UI.RawImage>().color = c;
                    }
                }
            }
        }
    }

    //shows/hides the information on the right side on tab press
    public void toggleControlInformationVisibility(HUDInfo temp_info)
    {
        bool currently_visible = sitting_right_side.transform.GetChild(1).gameObject.activeSelf;
        float a = 1.0f;
        if (currently_visible == false)
        {
            a = 0.1f;
        }
        sitting_right_side.transform.GetChild(0).GetChild(2).gameObject.SetActive(currently_visible);
        sitting_right_side.transform.GetChild(0).GetChild(3).gameObject.SetActive(!currently_visible);
        sitting_right_side.transform.GetChild(0).GetChild(4).GetComponent<CanvasGroup>().alpha = a;
        sitting_right_side.transform.GetChild(1).gameObject.SetActive(!currently_visible);
    }

    //updates shift direction UI indicator and get up indicator
    public void updateShiftIndicators(bool is_shifting, bool shiftable_position, bool can_shift_left, bool can_shift_right)
    {
        float a = 1.0f;
        if (is_shifting == true)
        {
            a = 0.1f;
        }
        sitting_left_side.transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(!is_shifting);
        sitting_left_side.transform.GetChild(1).GetChild(3).GetComponent<CanvasGroup>().alpha = a;
        sitting_left_side.transform.GetChild(2).gameObject.SetActive(shiftable_position);
        sitting_left_side.transform.GetChild(2).GetChild(2).GetChild(0).gameObject.SetActive(can_shift_left && !is_shifting);
        sitting_left_side.transform.GetChild(2).GetChild(3).GetChild(0).gameObject.SetActive(can_shift_right && !is_shifting);
        sitting_left_side.transform.GetChild(2).GetChild(4).GetComponent<CanvasGroup>().alpha = a;
    }

    //helper method that estimates the length of a control description based on the length of the description of that control's description
    private int getControlInfoOffset(HUDInfo temp_info)
    {
        return Mathf.Max(280, temp_info.getInfo().Length * 4);
    }

    //updates right side control info (description)
    public void updateSecondaryControlInformation(HUDInfo temp_info)
    {
        //determine whether to show or hide right side
        sitting_right_side.SetActive(temp_info.hasInfo());

        //set info frame title and description
        sitting_right_side.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().SetText(temp_info.getName());
        sitting_right_side.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().SetText(temp_info.getInfo());
        
        //resize based on length of control description
        int offset = getControlInfoOffset(temp_info);
        Transform control_info_frame = sitting_right_side.transform.GetChild(1);

        //background
        control_info_frame.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-285f, -360f + offset);
        control_info_frame.GetChild(0).GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(50f, -360f + offset);
        control_info_frame.GetChild(0).GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -410f + (offset / 2));
        control_info_frame.GetChild(0).GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(670f, offset);

        //border/divider
        control_info_frame.GetChild(1).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-290f, -355f + offset);
        control_info_frame.GetChild(1).GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -305f + offset);
        control_info_frame.GetChild(1).GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -440f + offset);
        control_info_frame.GetChild(1).GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(-340f, -475f + (offset / 2));
        control_info_frame.GetChild(1).GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(10f, offset - 180f);
        control_info_frame.GetChild(1).GetChild(6).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -520f + offset);

        //text
        control_info_frame.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -420f + offset);
        control_info_frame.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -515f + (offset / 2));
    }
    
    //updates the four blue dots in bottom right corner
    public void updatePowerConsumption(HUDInfo temp_info)
    {
        float power_consumption = temp_info.getPowerConsumption();
        if (power_consumption == displayed_power)
        {
            return;
        }

        //make power icon blue if consuming any power
        primary_default_power_circles.transform.GetChild(0).GetChild(0).gameObject.SetActive(power_consumption > 0.0f);

        //adjust circles
        float tmp_pwr = (power_consumption * 2.0f);
        for (int i = 0; i < 5; i++)
        {
            tmp_pwr = (power_consumption* 2.0f) - (0.2f * i);
            float a = tmp_pwr / 0.2f;
            primary_default_power_circles.transform.GetChild(i + 1).GetChild(0).GetComponent<UnityEngine.UI.Image>().fillAmount = a;
        }
        displayed_power = power_consumption;
    }

    public void updateInfoOverlayOffset(float offset)
    {
        station_functions.transform.localPosition = new Vector3(0.0f, offset, 0.0f);
    }

    public void checkStationFunctionsInput(bool force_hide)
    {
        bool inputted = PrimaryScript.checkInputIndexDown(15);

        if ((force_hide == false && inputted == false) || permanent_overlay_top_accessible == false || stations_button.transform.childCount == 0)
        {
            return;
        }

        bool hide = station_functions.activeSelf;
        station_functions.SetActive(!hide && !force_hide);
        float a = 1.0f;
        if (!hide && !force_hide)
        {
            a = 0.1f;
        }
        stations_button.transform.GetChild(2).gameObject.SetActive(hide || force_hide);
        stations_button.transform.GetChild(3).gameObject.SetActive(!hide && !force_hide);
        stations_button.transform.GetChild(4).GetComponent<CanvasGroup>().alpha = a;
        GetComponent<PrimaryScript>().setCursorVisibility(hide && !force_hide);
    }

    public void displayPopupDialogue(string speaker_name, string dialogue_content, Color dialogue_color, float display_time)
    {
        DialogueInfo dialogue_info = new DialogueInfo(speaker_name, "\"" + dialogue_content.ToUpper() + "\"", dialogue_color, display_time);
        if (popup_notification_queue.Count == 0 && isDisplayingPopupNotification() == false && dialogue_overlay.activeSelf == false)
        {
            popup_animation_coroutine = StartCoroutine(popupDialogueAnimation(dialogue_info));
        }
        else
        {
            popup_dialogue_queue.Add(dialogue_info);
        }
    }

    public void displayPopupNotification(int popup_index)
    {
        if (popup_notification_queue.Count == 0 && isDisplayingPopupNotification() == false && dialogue_overlay.activeSelf == false)
        {
            popup_animation_coroutine = StartCoroutine(popupNotificationAnimation(popup_index));
        }
        else
        {
            popup_notification_queue.Add(popup_index);
        }
    }

    public bool isDisplayingPopupNotification()
    {
        for (int i = 3; i < permanent_overlay.transform.childCount; i++)
        {
            if (permanent_overlay.transform.GetChild(i).gameObject.activeSelf == true)
            {
                return true;
            }
        }
        return false;
    }

    public void clearPopupNotifications()
    {
        //show stations button and current station indicator
        setPermanentOverlayTopVisibility(permanent_overlay_top_accessible);

        //clear pop-up queue
        popup_notification_queue.Clear();

        //hide all pop-up notifications
        for (int i = 3; i < permanent_overlay.transform.childCount; i++)
        {
            permanent_overlay.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void checkForPopupNotificationToDisplay()
    {
        if (popup_animation_coroutine == null && popup_notification_queue.Count > 0)
        {
            popup_animation_coroutine = StartCoroutine(popupNotificationAnimation(popup_notification_queue[0]));
        }
        else if (popup_animation_coroutine == null && popup_dialogue_queue.Count > 0)
        {
            popup_animation_coroutine = StartCoroutine(popupDialogueAnimation(popup_dialogue_queue[0]));
            popup_dialogue_queue.RemoveAt(0);
        }
    }

    IEnumerator popupNotificationAnimation(int popup_index)
    {
        GameObject popup_notification = permanent_overlay.transform.GetChild(3 + popup_index).gameObject;
        PrimaryScript.Instance.deactivate(true, false);
        setSecondaryInfoVisibility(true);
        setPermanentOverlayVisibility(true);
        setPermanentOverlayTopVisibility(false);

        foreach (Transform t in popup_notification.transform)
        {
            t.GetComponent<CanvasGroup>().alpha = 0.0f;
        }
        popup_notification.gameObject.SetActive(true);

        for (int i = 0; i < popup_notification.transform.childCount; i++)
        {
            CanvasGroup cg = popup_notification.transform.GetChild(i).GetComponent<CanvasGroup>();
            float anim_time = 1.0f;
            while (anim_time > 0.0f && PrimaryScript.Instance.isPaused() == false)
            {
                anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

                cg.alpha = 1.0f - anim_time;

                yield return null;
            }
            if (PrimaryScript.Instance.isPaused() == true)
            {
                break;
            }
        }

        while (PrimaryScript.checkInputIndexDown(13) == false && PrimaryScript.Instance.isPaused() == false)
        {
            yield return null;
        }

        popup_notification_queue.Remove(popup_index);
        popup_notification.SetActive(false);
        if (PrimaryScript.Instance.isPaused() == true)
        {
            clearPopupNotifications();
        }

        yield return null;

        if (PrimaryScript.Instance.getPlayerCamera().activeInHierarchy == true)
        {
            PrimaryScript.Instance.activate();
        }
        setPermanentOverlayTopVisibility(permanent_overlay_top_accessible);
        popup_animation_coroutine = null;
        checkForPopupNotificationToDisplay();
    }

    IEnumerator popupDialogueAnimation(DialogueInfo dialogue_info)
    {
        setSecondaryInfoVisibility(true);

        CanvasGroup dialogue_overlay_canvas_group = dialogue_overlay.GetComponent<CanvasGroup>();
        dialogue_overlay_canvas_group.alpha = 0.0f;
        foreach (Transform t in dialogue_overlay.transform.GetChild(1))
        {
            t.GetComponent<UnityEngine.UI.RawImage>().color = dialogue_info.dialogue_color;
        }
        dialogue_overlay.transform.GetChild(2).GetComponent<TMP_Text>().color = dialogue_info.dialogue_color;
        dialogue_overlay.transform.GetChild(2).GetComponent<TMP_Text>().SetText(dialogue_info.dialogue_speaker.ToUpper());
        TMP_Text overlay_text_content = dialogue_overlay.transform.GetChild(3).GetComponent<TMP_Text>();
        overlay_text_content.SetText("");
        dialogue_overlay.SetActive(true);

        float fade_time = 0.5f;
        while (fade_time > 0.0f && PrimaryScript.Instance.isPaused() == false)
        {
            fade_time -= Time.deltaTime;

            dialogue_overlay_canvas_group.alpha = 1.0f - (fade_time / 0.5f);

            yield return null;
        }

        dialogue_overlay.GetComponent<AudioSource>().Play();
        int iterator = 0;
        while (iterator < dialogue_info.dialogue_content.Length && PrimaryScript.Instance.isPaused() == false)
        {
            iterator++;
            overlay_text_content.SetText(dialogue_info.dialogue_content.Substring(0, iterator));

            yield return new WaitForSeconds(0.04f);
        }
        dialogue_overlay.GetComponent<AudioSource>().Stop();

        float delay_time = dialogue_info.display_time;
        while (delay_time > 0.0f)
        {
            delay_time -= Time.deltaTime;
            if (PrimaryScript.Instance.isPaused() == true)
            {
                break;
            }

            yield return null;
        }

        fade_time = 0.5f;
        while (fade_time > 0.0f && PrimaryScript.Instance.isPaused() == false)
        {
            fade_time -= Time.deltaTime;

            dialogue_overlay_canvas_group.alpha = fade_time / 0.5f;

            yield return null;
        }

        dialogue_overlay.SetActive(false);

        yield return null;

        popup_animation_coroutine = null;
        checkForPopupNotificationToDisplay();
    }
}