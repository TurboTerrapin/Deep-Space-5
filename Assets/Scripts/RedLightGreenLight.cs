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
using Unity.Netcode;

public class RedLightGreenLight : MonoBehaviour
{
    private Transform Camera;
    Vector3 OriginalCameraPosition;
    bool FriendlyTransmissionRecieved;
    bool isCameraShaking = false;
    bool ScenarioEndpointReached = false;
    private ImpulseThrottle impulse;
    private UniversalCommunicator communicator;
    private ScanWaveManager scanWaveManager;
    private ShipHealth shipHealth;
    Coroutine currentShake = null;

    //--SCAN WAVE INFORMATION--//

    //CENTER OF WAVE
    public Texture center_texture;
    public Color center_color;
    public float center_speed = 50.0f;

    //WAVE RINGS
    public List<Texture> ring_textures = null;
    public List<Color> ring_colors = null;
    public List<bool> ring_is_solid = null;
    public List<float> ring_speeds = null;

    void Start()
    {
        GameObject controlHandler = GameObject.FindWithTag("ControlHandler");
        impulse = controlHandler.GetComponent<ImpulseThrottle>();
        communicator = controlHandler.GetComponent<UniversalCommunicator>();

        GameObject sensorHandler = GameObject.FindWithTag("SensorHandler");
        scanWaveManager = sensorHandler.GetComponent<ScanWaveManager>();

        GameObject spaceship = GameObject.FindWithTag("Spaceship");
        shipHealth = spaceship.GetComponent<ShipHealth>();

        WaveInfo rlgl_wave = new WaveInfo();
        rlgl_wave.setCenter(center_texture, center_color, center_speed);
        rlgl_wave.setRings(ring_textures.Count, ring_colors, ring_textures, ring_is_solid, ring_speeds);

        scanWaveManager.initializeWave(0, rlgl_wave);

        /*
        if (sensorHandler.GetComponentAtIndex(0) is IControllable){

        }
        */

        Camera = GameObject.Find("Main Camera").transform;
        OriginalCameraPosition = Camera.localPosition;

        if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(RLGL());
        }
    }

    void Update()
    {
        if (isFriendlyMessage())
        {
            Debug.Log("Friendly message received.");
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

        while (ScenarioEndpointReached == false && shipHealth.getHullIntegrity() > 0.0f)
        {
            // Red light state
            FriendlyTransmissionRecieved = false;
            yield return StartCoroutine(RedLightState());

            if (ScenarioEndpointReached == true || shipHealth.getHullIntegrity() <= 0.0f)
            {
                break;
            }

            // Green light state
            float GreenLightDelay = Random.Range(15f, 30f);
            scanWaveManager.resizeWave(0, true, 0.5f);
            yield return new WaitForSeconds(GreenLightDelay);
            scanWaveManager.resizeWave(0, false, 2f);
        }
    }

    IEnumerator RedLightState()
    {
        FriendlyTransmissionRecieved = false;
        Debug.Log("RED LIGHT");

        while (FriendlyTransmissionRecieved == false && shipHealth.getHullIntegrity() > 0.0f)
        {
            // if the ship is moving
            if (impulse.getCurrentImpulse() > 0)
            {
                if (currentShake == null)
                {
                    isCameraShaking = true;
                    // Camera Shake(intensity)
                    currentShake = StartCoroutine(CameraShake(0.025f));
                }

                // Damage every second
                yield return new WaitForSeconds(1f);
                shipHealth.damageAllSections(2.5f);
                Debug.Log($"Ship Health: {shipHealth.getHullIntegrity()}");
                Debug.Log($"Impulse: {impulse.getCurrentImpulse()}");
            }
            else
            {
                if (currentShake != null)
                {
                    isCameraShaking = false;
                    StopCoroutine(currentShake);
                    currentShake = null;
                    Camera.localPosition = OriginalCameraPosition;
                }

                yield return null;
            }
        }

        if (currentShake != null)
        {
            isCameraShaking = false;
            StopCoroutine(currentShake);
            currentShake = null;
        }

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
