/*
    IntroSequenceTutorialTriggerHandler.cs
    - Used to handle displaying tutorial notifications during the intro sequence
    Contributor(s): Jake Schott
    Last Updated: 9/27/2026
*/

using UnityEngine;

public class IntroSequenceTutorialTriggerHandler : MonoBehaviour
{
    [SerializeField]
    private int tutorial_index = 0;
    private bool tutorial_shown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (tutorial_shown == false && PrimaryScript.Instance.hintsEnabled() == true && PrimaryScript.Instance.getHUD() < 2)
        {
            tutorial_shown = true;
            PrimaryScript.Instance.GetComponent<SecondaryScript>().displayIntroSequenceTutorialNotification(tutorial_index);
            gameObject.SetActive(false);
        }
    }
}
