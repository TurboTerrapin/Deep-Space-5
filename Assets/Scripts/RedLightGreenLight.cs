/*
    RedLightGreenLight.cs

    1. 60 second delay before RedLightGreenLight commences.
    2. While ship health is not 0, the red light state begins until a friendly transmission is recieved.
    3. Once the friendly transmission is recieved, it will remain in the green light state for 15-30 seconds.
    4. Steps 2 and 3 will loop until the end point of the scenario is reached or the ship health is 0.

    Red Light Phase: Camera shake with damage taken to ship.
    Green Light Phase: Enemy does nothing.

*/

using UnityEngine;
using System.Collections;

public class RedLightGreenLight : MonoBehaviour
{
    public Transform CameraTransform;
    Vector3 OriginalCameraPosition;
    int ShipHealth = 100;
    bool FriendlyTransmissionRecieved;
    bool isCameraShaking = false;
    bool ScenarioEndpointReached = false;

    void Start()
    {
        StartCoroutine(RLGL());
        if (CameraTransform == null)
        {
            CameraTransform = Camera.current.transform;
        }
        
        OriginalCameraPosition = CameraTransform.localPosition;
        
    }

    void Update()
    {
        if (CameraTransform == null)
        {
            CameraTransform = Camera.current.transform;
            OriginalCameraPosition = CameraTransform.localPosition;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            FriendlyTransmissionRecieved = true;
        }
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
            isCameraShaking = true;
            // Camera Shake(intensity)
            StartCoroutine(CameraShake(0.1f));

            // Damage every second
            yield return new WaitForSeconds(1f);
            ShipHealth -= 1;
            Debug.Log($"Ship Health: {ShipHealth}");

            // Check for friendly transmission
        }
        isCameraShaking = false;
        CameraTransform.localPosition = OriginalCameraPosition;
        Debug.Log("GREEN LIGHT");
    }

    IEnumerator CameraShake(float intensity)
    {
        Debug.Log("Shaking: " + CameraTransform.localPosition + " + " + (Random.insideUnitSphere * intensity));

        while (isCameraShaking == true)
        {
            Vector3 Shake = Random.insideUnitSphere * intensity;
            CameraTransform.localPosition = OriginalCameraPosition + Shake;
            yield return null;
        }

        CameraTransform.localPosition = OriginalCameraPosition;
    }
}
