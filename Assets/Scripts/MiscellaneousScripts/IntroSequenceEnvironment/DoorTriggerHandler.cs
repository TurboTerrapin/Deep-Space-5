/*
    DoorTriggerHandler.cs
    - Used to handle door opening and closing animation
    Contributor(s): Jake Schott
    Last Updated: 8/30/2026
*/

using UnityEngine;
using System.Collections;

public class DoorTriggerHandler : MonoBehaviour
{
    public GameObject door_left;
    public GameObject door_right;

    [SerializeField]
    private float door_animation_time = 1.0f;
    [SerializeField]
    private Vector3 door_left_open_pos;
    [SerializeField]
    private Vector3 door_right_open_pos;
    private Coroutine open_coroutine = null;
    private Coroutine close_coroutine = null;
    private bool currently_open = false;

    IEnumerator doorAnimation(Vector3 left_dest_pos, Vector3 right_dest_pos)
    {
        Vector3 starting_pos_left = door_left.transform.localPosition;
        Vector3 starting_pos_right = door_right.transform.localPosition;

        GetComponent<AudioSource>().Play();

        float anim_time = door_animation_time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            door_left.transform.localPosition = Vector3.Lerp(left_dest_pos, starting_pos_left, anim_time / door_animation_time);
            door_right.transform.localPosition = Vector3.Lerp(right_dest_pos, starting_pos_right, anim_time / door_animation_time);

            yield return null;
        }

        open_coroutine = null;
        close_coroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (open_coroutine == null && currently_open == false)
        {
            if (close_coroutine != null)
            {
                StopCoroutine(close_coroutine);
                close_coroutine = null;
            }
            currently_open = true;
            open_coroutine = StartCoroutine(doorAnimation(door_left_open_pos, door_right_open_pos));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (close_coroutine == null && (open_coroutine != null || currently_open == true))
        {
            if (open_coroutine != null)
            {
                StopCoroutine(open_coroutine);
                open_coroutine = null;
            }
            currently_open = false;
            close_coroutine = StartCoroutine(doorAnimation(Vector3.zero, Vector3.zero));
        }
    }
}
