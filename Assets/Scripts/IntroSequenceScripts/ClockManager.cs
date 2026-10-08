/*
    ClockManager.cs
    - Used to update the clocks in the intro sequence
    Contributor(s): Jake Schott
    Last Updated: 8/31/2026
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ClockManager : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float STARTING_TIME = 12600.0f; //seconds after midnight

    public List<TMP_Text> clock_texts = null;
    public List<GameObject> second_hands = null;
    public List<GameObject> minute_hands = null;
    public List<GameObject> hour_hands = null;

    private float current_time = STARTING_TIME;

    private void Update()
    {
        current_time += Time.deltaTime;

        if (current_time >= 86400.0f)
        {
            current_time -= 86400.0f;
        }

        float minutes = (current_time % 3600.0f) / 3600.0f;
        float hours = current_time / 3600.0f;

        //adjust analog clocks
        Quaternion q = Quaternion.Euler(0.0f, 0.0f, ((current_time % 60.0f) / 60.0f) * 360.0f);
        foreach (GameObject s in second_hands)
        {
            s.transform.localRotation = q;
        }

        q = Quaternion.Euler(0.0f, 0.0f, minutes * 360.0f);
        foreach (GameObject m in minute_hands)
        {
            m.transform.localRotation = q;
        }

        q = Quaternion.Euler(0.0f, 0.0f, ((hours % 12.0f) / 12.0f) * 360.0f);
        foreach (GameObject h in hour_hands)
        {
            h.transform.localRotation = q;
        }

        //adjust digital clocks and text strings
        string minutes_time = Mathf.FloorToInt(minutes * 60.0f).ToString();
        if (minutes_time.Length < 2)
        {
            minutes_time = "0" + minutes_time;
        }

        string hours_time = Mathf.FloorToInt(hours).ToString();
        if (hours_time.Length < 2)
        {
            hours_time = "0" + hours_time;
        }

        string clock_time = hours_time + ":" + minutes_time;
        foreach (TMP_Text t in clock_texts)
        {
            t.SetText(clock_time);
        }
    }
}