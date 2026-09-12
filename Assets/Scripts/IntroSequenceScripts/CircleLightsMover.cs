/*
    CircleLightsMover.cs
    - Used to loop the circle lights in item storage
    Contributor(s): Jake Schott
    Last Updated: 9/11/2026
*/

using UnityEngine;
using System.Collections;

public class CircleLightsMover : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float MOVE_TIME = 1.0f;
    private static Vector3 END_POSITION = new Vector3(0.0f, 40.0f, 0.0f);

    private void Start()
    {
        StartCoroutine(circleLightsLooper());
    }

    IEnumerator circleLightsLooper()
    {
        while (true)
        {
            float anim_time = MOVE_TIME;
            while (anim_time > 0.0f)
            {
                anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

                transform.localPosition = Vector3.Lerp(END_POSITION, Vector3.zero, anim_time / MOVE_TIME);

                yield return null;
            }
        }
    }
}