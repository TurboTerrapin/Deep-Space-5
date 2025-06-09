/*
    RedLightGreenLight.cs
    Contributor: Beata Musial

    1. 60 second delay before RedLightGreenLight commences.
    2. While ship health is not 0, the red light state begins until a friendly transmission is recieved.
    3. Once the friendly transmission is recieved, it will remain in the green light state for 15-30 seconds.
    4. Steps 2 and 3 will loop until the end point of the scenario is reached or the ship health is 0.

    Red Light Phase: Camera shake with damage taken to ship only while impulse > 0;
    Green Light Phase: Enemy does nothing.

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RedLightGreenLight : MonoBehaviour
{
    public Transform Camera;
    Vector3 OriginalCameraPosition;
    int ShipHealth = 100;
    bool FriendlyTransmissionRecieved;
    bool isCameraShaking = false;
    bool ScenarioEndpointReached = false;
    private ImpulseThrottle impulse;
    private UniversalCommunicator communicator;

    void Start()
    {
        GameObject controlHandler = GameObject.FindWithTag("ControlHandler");
        impulse = controlHandler.GetComponent<ImpulseThrottle>();
        communicator = controlHandler.GetComponent<UniversalCommunicator>();

        OriginalCameraPosition = Camera.localPosition;
        StartCoroutine(RLGL());
    }

    void Update()
    {
        if (isFriendlyMessage())
        {
            FriendlyTransmissionRecieved = true;
        }

        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    FriendlyTransmissionRecieved = true;
        //}
    }

    IEnumerator RLGL()
    {
        // 60 second delay
        //yield return new WaitForSeconds(60f);

        while (ScenarioEndpointReached == false && ShipHealth != 0)
        {
            // Red light state
            FriendlyTransmissionRecieved = false;
            yield return StartCoroutine(RedLightState());

            if (ScenarioEndpointReached == true || ShipHealth == 0)
            {
                break;
            }

            // Green light state
            float GreenLightDelay = Random.Range(15f, 30f);
            yield return new WaitForSeconds(GreenLightDelay);
        }
    }

    IEnumerator RedLightState()
    {
        Debug.Log("RED LIGHT");

        while (FriendlyTransmissionRecieved == false)
        {
            if (impulse.getCurrentImpulse() > 0)
            {
                isCameraShaking = true;
                // Camera Shake(intensity)
                StartCoroutine(CameraShake(0.1f));

                // Damage every second
                yield return new WaitForSeconds(1f);
                ShipHealth -= 1;
                Debug.Log($"Ship Health: {ShipHealth}");

                // Check for friendly transmission
            }

            isCameraShaking = true;
            // Camera Shake(intensity)
            StartCoroutine(CameraShake(0.1f));

            // Damage every second
            yield return new WaitForSeconds(1f);
            ShipHealth -= 1;
            Debug.Log($"Ship Health: {ShipHealth}");
        }
        isCameraShaking = false;
        Camera.localPosition = OriginalCameraPosition;
        Debug.Log("GREEN LIGHT");
    }

    IEnumerator CameraShake(float intensity)
    {
        Debug.Log("Shaking: " + Camera.localPosition + " + " + (Random.insideUnitSphere * intensity));

        while (isCameraShaking == true)
        {
            Vector3 Shake = Random.insideUnitSphere * intensity;
            Camera.localPosition = OriginalCameraPosition + Shake;
            yield return null;
        }

        Camera.localPosition = OriginalCameraPosition;
    }

    private bool isFriendlyMessage()
    {
        List<int> codeIndex = communicator.CodeIndex;

        if (codeIndex.Count != 8)
        {
            return false;
        }

        int[] friendlyMessage = { 1, 0, 0, 1, 1, 0, 0, 1 };

        for (int i = 0; i < 8; i++)
        {
            if (codeIndex[i] != friendlyMessage[i])
            {
                return false;
            }
        }
        return true;
    }

}
