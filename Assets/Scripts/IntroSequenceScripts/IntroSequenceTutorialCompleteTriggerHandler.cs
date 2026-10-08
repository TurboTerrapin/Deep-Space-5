/*
    IntroSequenceTutorialCompleteTriggerHandler.cs
    - Ends the tutorial when touched
    Contributor(s): Jake Schott
    Last Updated: 9/28/2026
*/

using UnityEngine;

public class IntroSequenceTutorialCompleteTriggerHandler : MonoBehaviour
{
    private bool tutorial_complete = false;

    private void OnTriggerEnter(Collider other)
    {
        if (tutorial_complete == false)
        {
            tutorial_complete = true;
            ReferenceAssistor.Instance.intro_sequence_manager.endTutorial();
            gameObject.SetActive(false);
        }
    }
}
