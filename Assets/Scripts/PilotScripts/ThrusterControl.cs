/*
    ThrusterControl.cs
    - Defines binary left/right/up/down structure for thrusters
    - Handles physical buttons
    - Meant to be extended
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class ThrusterControl : MonoBehaviour 
{
    public List<Transform> physical_buttons;
    public GameObject display_canvas;

    protected bool[] buttons = new bool[2];
    protected Vector3[] button_positions = new Vector3[2];
    protected int[] button_increments = new int[2];
    protected float thrust_direction = 0;

    private void Start()
    {
        buttons[0] = false; //down, right
        buttons[1] = false; //up, left
        button_increments[0] = 0;
        button_increments[1] = 0;
        button_positions[0] = physical_buttons[0].localPosition;
        button_positions[1] = new Vector3(0f, -0.01864f, -17.89135f);
    }
    protected void adjustThrust(float new_thrust)
    {
        thrust_direction = new_thrust;
    }
    protected void adjustButton(Transform button, int button_index)
    {
        if (buttons[button_index] == false)
        {
            if (button_increments[button_index] > 0)
            {
                button_increments[button_index]--;
            }
        }
        else
        {
            if (button_increments[button_index] < 10)
            {
                button_increments[button_index]++;
            }
        }
        if (buttons[0] == false && buttons[1] == false && button_increments[0] < 10 && button_increments[1] < 10 && thrust_direction != 0)
        {
            adjustThrust(0.0f);
        }
        int starting_index = 0;
        if (button_index == 0)
        {
            starting_index = 10;
        }
        for (int i = starting_index; i <= starting_index + 10; i++) 
        {
            bool visible = false;
            if (button_increments[button_index] >= i - starting_index) 
            {
                visible = true;
            }
            display_canvas.transform.GetChild(i + 1).gameObject.SetActive(visible);
        }
        int dist_from_zero = button_increments[button_index];
        physical_buttons[button_index].transform.localPosition = 
            new Vector3(button_positions[0].x + ((button_positions[1].x - button_positions[0].x) * (dist_from_zero / 10f)), 
            button_positions[0].y + ((button_positions[1].y - button_positions[0].y) * (dist_from_zero / 10f)), 
            button_positions[0].z + ((button_positions[1].z - button_positions[0].z) * (dist_from_zero / 10f)));
    }
}
