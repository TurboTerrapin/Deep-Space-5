/*
    TransmissionHandler.cs
    - Moves the waves
    Contributor(s): Jake Schott
    Last Updated: 5/8/2025
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class TransmissionHandler : MonoBehaviour
{
    public GameObject frequency_text;
    public GameObject transmission_canvas;
    public List<GameObject> waves = null;

    private List<string> frequencies = new List<string>() { "120.5", "126.1", "129.4", "129.8", "134.3", "139.9" };
    private List<int> corresponding_waves = new List<int>() { 4, 4, 4, 4, 5, 4 };
    private int frequency_index = 0;
    private float move_speed = 1.0f;

    public int updateFrequency(int dir)
    {
        if (dir == 0) //decrease
        {
            frequency_index--;
            if (frequency_index < 0)
            {
                frequency_index = frequencies.Count - 1;
            }
        }
        else //increase
        {
            frequency_index++;
            if (frequency_index > frequencies.Count - 1)
            {
                frequency_index = 0;
            }
        }
        return frequency_index;
    }
    public bool setFrequency(int new_freq)
    {
        if (new_freq > 0 && new_freq <= frequencies.Count - 1)
        {
            frequency_index = new_freq;
            return true;
        }
        return false;
    }
    public void adjustSpeed(bool slow)
    {
        if (slow == true)
        {
            move_speed = 0.5f;
        }
        else
        {
            move_speed = 1.0f;
        }
    }
    public void updateDisplay()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].GetComponent<UnityEngine.UI.RawImage>().texture = transmission_canvas.transform.GetChild(corresponding_waves[frequency_index]).gameObject.GetComponent<UnityEngine.UI.RawImage>().mainTexture;
        }
        frequency_text.GetComponent<TMP_Text>().SetText(frequencies[frequency_index] + "Mh");
    }
    void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].GetComponent<RectTransform>().localPosition -= new Vector3(0f, 0.05f * Time.deltaTime * move_speed, 0f);
        }
        if (waves[0].GetComponent<RectTransform>().localPosition.y <= -0.109f)
        {
            waves[0].GetComponent<RectTransform>().localPosition = new Vector3(0f, 0.114f, 0f);
            GameObject temp_wave = waves[0];
            waves.RemoveAt(0);
            waves.Add(temp_wave);
        }
    }
}
