/*
    IntroSequenceTutorialTriggerHandler.cs
    - Used to handle displaying pop-up notifications during the intro sequence
    Contributor(s): Jake Schott
    Last Updated: 10/5/2026
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
            PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupNotification(tutorial_index);
            gameObject.SetActive(false);
        }
    }
}
