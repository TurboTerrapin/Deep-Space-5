/*
    Throttle.cs
    - Handles inputs for impulse throttle
    - Moves throttle lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Throttle : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "IMPULSE THROTTLE";
    private List<string> CONTROL_DESCS = new List<string> {"DECREASE", "INCREASE"};
    private List<int> CONTROL_INDEXES = new List<int>() {4, 5};

    public GameObject handle;
    public GameObject display_canvas; //used to display the bars beneath the handle
    public GameObject speed_information; //used to update the speedometer

    private float impulse = 0.0f;
    private float temp_impulse;
    private Vector3 initial_pos; //handle starting position (0% impulse)
    private Vector3 final_pos; //handle final position (100% impulse)

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setInputs(CONTROL_DESCS, CONTROL_INDEXES);

        initial_pos = handle.transform.position;
        final_pos = new Vector3(0f, 0.0869f, -18.1262f);
    }
    public HUDInfo getHUDinfo()
    {
        return hud_info;
    }
    private void adjustImpulse() //adjusts physical position of lever and impulse value after inputs
    {
        if (temp_impulse != impulse) //throttle was adjusted
        {
            int impulse_as_int = (int)(impulse * 100.0f);
            if (impulse_as_int < 100)
            { 
                if (temp_impulse < impulse)
                {
                    for (int i = 20; i > (impulse_as_int / 5) + 1; i--)
                    {
                        if (display_canvas.transform.childCount > i && i != 1)
                        {
                            Destroy(display_canvas.transform.GetChild(i).gameObject);
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= impulse_as_int / 5; i++)
                    {
                        if (display_canvas.transform.childCount <= i + 1)
                        {
                            GameObject new_bar = UnityEngine.Object.Instantiate(display_canvas.transform.GetChild(1).gameObject, display_canvas.transform);
                            new_bar.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.93f, 1.0f, 1.0f);
                            new_bar.transform.localPosition = new Vector3(0f, -0.1467f + i * 0.0151f, 0f);
                            new_bar.name = "Rectangle" + i;
                        }
                    }
                }
                if (display_canvas.transform.childCount > (impulse_as_int / 5) + 1)
                {
                    display_canvas.transform.GetChild((impulse_as_int / 5) + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.93f, 1.0f, (0.2f * (impulse_as_int % 5)));
                }
            }
            impulse = temp_impulse;
            handle.transform.position = new Vector3(initial_pos.x + ((final_pos.x - initial_pos.x) * impulse), initial_pos.y + ((final_pos.y - initial_pos.y) * impulse), initial_pos.z + ((final_pos.z - initial_pos.z) * impulse));
            speed_information.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().SetText("" + Mathf.Round(impulse * 100.0f));
        }
    }
    private void increment()
    {
        temp_impulse += 0.002f * (impulse / 0.5f) + 0.001f;
        temp_impulse = Mathf.Min(temp_impulse, 1.0f);
    }
    private void decrement()
    {
        temp_impulse -= 0.002f * (impulse / 0.5f) + 0.001f;
        temp_impulse = Mathf.Max(temp_impulse, 0.0f);
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        if (inputs.Contains(KeyCode.E) || inputs.Contains(KeyCode.RightArrow)) //E to increment
        {
            increment();
        }
        if (inputs.Contains(KeyCode.Q) || inputs.Contains(KeyCode.LeftArrow))  //Q to decrement
        {
            decrement();
        }
        adjustImpulse();
    }
}
