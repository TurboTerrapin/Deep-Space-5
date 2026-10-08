/*
    SecondaryScript.cs
    - Helps with secondary info that isn't primary control interactions
    - Handles displaying pop-up notifications
    Contributor(s): Jake Schott
    Last Updated: 10/5/2026
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SecondaryScript : MonoBehaviour
{
    //CLASS CONSTANTS
    public static Color DEFAULT_BORDER_CORDER = new Color(0.12f, 0.12f, 0.12f, 1.0f);

    public GameObject secondary_info;

    private GameObject permanent_overlay;
    private GameObject stations_button;
    private GameObject current_station_indicator;
    private GameObject station_functions;
    private GameObject mission_objective;
    private GameObject sitting_overlay;
    private GameObject sitting_left_side;
    private GameObject sitting_right_side;
    private GameObject primary_default_power_circles;

    private List<int> popup_notification_queue = new List<int>();
    private float displayed_power = 0.0f;
    private Coroutine popup_notification_animation_coroutine = null;

    private void Awake()
    {
        permanent_overlay = secondary_info.transform.GetChild(0).gameObject;
        stations_button = permanent_overlay.transform.GetChild(0).gameObject;
        current_station_indicator = permanent_overlay.transform.GetChild(1).gameObject;
        station_functions = permanent_overlay.transform.GetChild(2).gameObject;
        mission_objective = permanent_overlay.transform.GetChild(3).gameObject;
        sitting_overlay = secondary_info.transform.GetChild(1).gameObject;
        sitting_left_side = sitting_overlay.transform.GetChild(0).gameObject;
        sitting_right_side = sitting_overlay.transform.GetChild(1).gameObject;
        primary_default_power_circles = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(3).GetChild(1).gameObject;
    }

    public void setSecondaryInfoVisibility(bool active)
    {
        secondary_info.SetActive(active);
    }

    public void setPermanentOverlayVisibility(bool active)
    {
        permanent_overlay.SetActive(active);
    }

    private void setPermanentOverlayTopVisibility(bool active)
    {
        stations_button.SetActive(active);
        current_station_indicator.SetActive(active);
    }

    public void setSittingOverlayVisibility(bool active)
    {
        sitting_overlay.SetActive(active);
    }

    public void setSittingRightSideVisibility(bool active)
    {
        sitting_right_side.SetActive(active);
    }

    //updates station indicator in top right as well as colors on top
    public void onStationChange(Color c, Texture icon)
    {
        //handle station functions top left indicator and station icon top right indicator
        if (SceneManager.GetActiveScene().name.CompareTo("IntroSequence") != 0)
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

        if ((SceneManager.GetActiveScene().name.CompareTo("IntroSequence") == 0) || (inputted == false && force_hide == false))
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


    public void displayPopupNotification(int popup_index)
    {
        if (popup_notification_queue.Count == 0 && isDisplayingPopupNotification() == false)
        {
            popup_notification_animation_coroutine = StartCoroutine(popupNotificationAnimation(popup_index));
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

    public bool isAnimatingPopupNotification()
    {
        return (popup_notification_animation_coroutine != null);
    }

    public bool hasPopupNotificationsInQueue()
    {
        return (popup_notification_queue.Count > 0);
    }

    private void clearPopupNotifications()
    {
        //show stations button and current station indicator
        setPermanentOverlayTopVisibility(true);

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
        if (popup_notification_animation_coroutine == null && popup_notification_queue.Count > 0)
        {
            popup_notification_animation_coroutine = StartCoroutine(popupNotificationAnimation(popup_notification_queue[0]));
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
        popup_notification.gameObject.SetActive(false);
        if (PrimaryScript.Instance.isPaused() == true)
        {
            clearPopupNotifications();
        }

        yield return null;

        PrimaryScript.Instance.activate();
        setPermanentOverlayTopVisibility(true);
        popup_notification_animation_coroutine = null;
        checkForPopupNotificationToDisplay();
    }
}