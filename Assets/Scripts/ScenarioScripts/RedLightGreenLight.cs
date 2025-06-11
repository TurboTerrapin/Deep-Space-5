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

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RedLightGreenLight : NetworkBehaviour, IUniversalCommunicable
{
    //CLASS CONSTANTS
    private static int MINIMUM_PLAYERS = 2;

    private Transform Camera;
    Vector3 OriginalCameraPosition;
    bool ScenarioEndpointReached = false;
    private ImpulseThrottle impulse;
    private UniversalCommunicator communicator;
    private ScanWaveManager scanWaveManager;
    private ShipHealth shipHealth;
    private Coroutine redLightCoroutine = null;
    private Coroutine greenLightCoroutine = null;
    private Coroutine cameraShakeCoroutine = null;

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
    //-------------------------//

    void Start()
    {
        GameObject controlHandler = GameObject.FindWithTag("ControlHandler");
        impulse = controlHandler.GetComponent<ImpulseThrottle>();
        communicator = controlHandler.GetComponent<UniversalCommunicator>();

        GameObject sensorHandler = GameObject.FindWithTag("SensorHandler");
        scanWaveManager = sensorHandler.GetComponent<ScanWaveManager>();

        GameObject spaceship = GameObject.FindWithTag("Spaceship");
        shipHealth = spaceship.GetComponent<ShipHealth>();

        WaveInfo RLGLwave = new WaveInfo();
        RLGLwave.setCenter(center_texture, center_color, center_speed);
        RLGLwave.setRings(ring_textures.Count, ring_colors, ring_textures, ring_is_solid, ring_speeds);

        scanWaveManager.initializeWave(0, RLGLwave);

        Camera = GameObject.Find("Main Camera").transform;
        OriginalCameraPosition = Camera.localPosition;

        if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(waitForOthers());
        }
    }

    IEnumerator waitForOthers()
    {
        while (NetworkManager.Singleton.ConnectedClientsIds.Count < MINIMUM_PLAYERS)
        {
            yield return null;
        }
        yield return new WaitForSeconds(5.0f);
        enterRedLightStateRPC();
    }

    IEnumerator GreenLightState()
    {
        //contract energy wave
        scanWaveManager.resizeWave(0, true, 0.5f);
        if (NetworkManager.Singleton.IsHost)
        {
            yield return new WaitForSeconds(Random.Range(15.0f, 30.0f));
            endGreenLightStateRPC(Random.Range(3.0f, 6.0f));
        }
    }

    IEnumerator EndGreenLight(float end_time)
    {
        //expand energy wave
        scanWaveManager.resizeWave(0, false, end_time);
        yield return new WaitForSeconds(end_time);
        if (NetworkManager.Singleton.IsHost && ScenarioEndpointReached == false)
        {
            enterRedLightStateRPC();
        }
    }

    IEnumerator RedLightState()
    {
        if (cameraShakeCoroutine == null)
        {
            cameraShakeCoroutine = StartCoroutine(CameraShakeState());
        }

        if (NetworkManager.Singleton.IsHost)
        {
            while (shipHealth.getHullIntegrity() > 0.0f && ScenarioEndpointReached == false)
            {
                // if the ship is moving
                if (impulse.getCurrentImpulse() > 0.0f)
                {
                    float time_before_damage_is_inflicted = 1.0f;
                    while (time_before_damage_is_inflicted > 0.0f && impulse.getCurrentImpulse() > 0.0f)
                    {
                        time_before_damage_is_inflicted -= Time.deltaTime;
                        yield return null;
                    }
                    if (impulse.getCurrentImpulse() > 0.0f)
                    {
                        shipHealth.damageAllSections(10.0f * impulse.getCurrentImpulse());
                    }
                }
                else
                {
                    yield return null;
                }
            }
            if (ScenarioEndpointReached == true)
            {
                Debug.Log("Scenario beaten!");
            }
            else if (shipHealth.getHullIntegrity() <= 0.0f)
            {
                Debug.Log("Nooooo the ship died :(");
            }
        }
    }

    IEnumerator CameraShakeState()
    {
        //only shakes when impulse is > 0, gets worse as impulse goes up
        while (true)
        {
            float intensity = impulse.getCurrentImpulse() * 0.025f;
            Vector3 Shake = Random.insideUnitSphere * intensity;
            Camera.localPosition = OriginalCameraPosition + Shake;
            yield return null;
        }
    }

    public void handleTransmission(List<int> code_indexes, List<int> code_colors, List<int> code_is_numeric)
    {
        if (NetworkManager.Singleton.IsHost && greenLightCoroutine == null)
        {
            bool successful_transmission = isFriendlyMessage(code_indexes, code_colors, code_is_numeric);
            if (successful_transmission)
            {
                enterGreenLightStateRPC();
            }
            else
            {
                //possibility to damage the ship if wrong message is sent, but does nothing for now
            }
        }
    }
    private bool isFriendlyMessage(List<int> ci, List<int> cc, List<int> cin)
    {
        if (ci.Count != 8)
        {
            return false;
        }

        int[] friendlyMessage = { 1, 0, 0, 1, 1, 0, 0, 1 };

        for (int i = 0; i < 8; i++)
        {
            if (ci[i] != friendlyMessage[i])
            {
                return false;
            }
        }
        return true;
    }

    private void resetCoroutines()
    {
        if (redLightCoroutine != null)
        {
            StopCoroutine(redLightCoroutine);
        }
        if (greenLightCoroutine != null)
        {
            StopCoroutine(greenLightCoroutine);
        }
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
        }
        redLightCoroutine = null;
        greenLightCoroutine = null;
        cameraShakeCoroutine = null;
    }

    [Rpc(SendTo.Everyone)]
    private void enterRedLightStateRPC()
    {
        resetCoroutines();
        redLightCoroutine = StartCoroutine(RedLightState());
    }

    [Rpc(SendTo.Everyone)]
    private void enterGreenLightStateRPC()
    {
        resetCoroutines();
        greenLightCoroutine = StartCoroutine(GreenLightState());
    }

    [Rpc(SendTo.Everyone)]
    private void endGreenLightStateRPC(float contraction_time)
    {
        resetCoroutines();
        greenLightCoroutine = StartCoroutine(EndGreenLight(contraction_time));
    }
}
