/*
    CircleLightsRotater.cs
    - Used to rotate the circle lights in item storage
    Contributor(s): Jake Schott
    Last Updated: 9/30/2026
*/

using UnityEngine;

public class CircleLightsRotater : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float ROTATE_TIME = 25.0f;

    public GameObject[] rotating_circle_lights = new GameObject[2];

    private void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            rotating_circle_lights[i].transform.Rotate(0.0f, 0.0f, ROTATE_TIME * Time.deltaTime, Space.World);
        }
    }
}