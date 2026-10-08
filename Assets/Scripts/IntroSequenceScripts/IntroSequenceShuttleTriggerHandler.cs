/*
    IntroSequenceShuttleTriggerHandler.cs
    - Used to launch the shuttle so it is seen when the player leaves their quarters
    Contributor(s): Jake Schott
    Last Updated: 9/28/2026
*/

using UnityEngine;

public class IntroSequenceShuttleTriggerHandler : MonoBehaviour
{
    public GameObject shuttle;

    private bool shuttle_launched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (shuttle_launched == false)
        {
            shuttle_launched = true;
            shuttle.gameObject.SetActive(true);
            Destroy(this);
        }
    }
}
