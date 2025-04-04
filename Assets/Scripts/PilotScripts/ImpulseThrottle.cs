/*
    ImpulseThrottle.cs
    - Handles inputs for impulse throttle
    - Moves throttle lever accordingly
    Contributor(s): Jake Schott
    Last Updated: 4/3/2025
*/

using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ImpulseThrottle : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "IMPULSE THROTTLE";
    private List<string> CONTROL_DESCS = new List<string> {"DECREASE", "INCREASE"};
    private List<int> CONTROL_INDEXES = new List<int>() {4, 5};

    public GameObject handle;
    public GameObject display_canvas; //used to display the bars beneath the handle
    public GameObject speed_information; //used to update the speedometer

    private float impulse = 0.0f;
    private int impulse_direction = 0; //0 is neutral, 1 is increase, and -1 is decrease
    private Vector3 initial_pos; //handle starting position (0% impulse)
    private Vector3 final_pos = new Vector3(0f, 0.0869f, -18.1262f); //handle final position (100% impulse)

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        hud_info.setInputs(CONTROL_DESCS, CONTROL_INDEXES);

        initial_pos = handle.transform.position; //sets the initial position
    }
    public HUDInfo getHUDinfo()
    {
        return hud_info;
    }
    private void displayAdjustment()
    {
        //update bars on screen
        int impulse_as_int = (int)(impulse * 100.0f);
        if (impulse_as_int < 100)
        { 
            display_canvas.transform.GetChild((impulse_as_int / 5) + 1).gameObject.GetComponent<UnityEngine.UI.RawImage>().color = new Color(0, 0.93f, 1.0f, (0.2f * (impulse_as_int % 5)));
        }

        //update lever position
        handle.transform.position = new Vector3(initial_pos.x + ((final_pos.x - initial_pos.x) * impulse), initial_pos.y + ((final_pos.y - initial_pos.y) * impulse), initial_pos.z + ((final_pos.z - initial_pos.z) * impulse));
        
        //update speedometer text
        speed_information.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().SetText("" + Mathf.Round(impulse * 100.0f));
    }
    void Update()
    {
        if (impulse_direction != 0)
        {
            if (impulse_direction > 0) 
            {
                impulse = Mathf.Min(1.0f, impulse + (0.002f * (impulse / 0.5f) + 0.001f) * Time.deltaTime * 50.0f);
            }
            else
            {
                impulse = Mathf.Max(0.0f, impulse - (0.002f * (impulse / 0.5f) + 0.001f) * Time.deltaTime * 50.0f);
            }
            displayAdjustment();
        }
    }
    public void handleInputs(List<KeyCode> inputs)
    {
        int temp_impulse_direction = 0;
        if (inputs.Contains(KeyCode.E) || inputs.Contains(KeyCode.RightArrow)) //E to increment
        {
            temp_impulse_direction += 1;
        }
        if (inputs.Contains(KeyCode.Q) || inputs.Contains(KeyCode.LeftArrow))  //Q to decrement
        {
            temp_impulse_direction -= 1;
        }
        impulse_direction = temp_impulse_direction;
    }
}
