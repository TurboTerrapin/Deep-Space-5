/*
    ShuttleMover.cs
    - Used to loop the flying shuttle on the outside of the intro sequence hallway
    Contributor(s): Jake Schott
    Last Updated: 9/8/2026
*/

using UnityEngine;
using System.Collections;

public class ShuttleMover : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float SHUTTLE_TRAVEL_TIME = 12.0f;

    public Vector3 end_position;

    private Vector3 start_position;

    private void Start()
    {
        start_position = transform.position;

        StartCoroutine(shuttleLooper());
    }

    IEnumerator shuttleLooper()
    {
        while (true)
        {
            yield return StartCoroutine(shuttleTrip());
            yield return new WaitForSeconds(Random.Range(10.0f, 15.0f));
        }
    }

    IEnumerator shuttleTrip()
    {
        float anim_time = SHUTTLE_TRAVEL_TIME;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            transform.position = Vector3.Lerp(end_position, start_position, anim_time / SHUTTLE_TRAVEL_TIME);

            yield return null;
        }
    }
}