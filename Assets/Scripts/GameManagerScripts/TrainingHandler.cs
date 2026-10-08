/*
    TrainingHandler.cs
    - Handles loading into TrainingEnvironment scene
    Contributor(s): Jake Schott
    Last Updated: 10/4/2026
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;

public class TrainingHandler : MonoBehaviour
{
    //CLASS CONSTANTS
    private static Vector3 PLAYER_STARTING_POSITION = new Vector3(0.0f, 0.079f, -10.75f);
    private static float SHIP_SETBACK_DISTANCE = 250.0f; //how far back the ship is at the start

    public GameObject training_hud_display;
    public GameObject training_obstacles;

    private int simulation_number = 1;

    private void Start()
    {
        StartCoroutine(trainingEnvironmentInitializer());
    }

    IEnumerator trainingEnvironmentInitializer()
    {
        LoadHandler load_handler = GameObject.Find("LoadHandler").GetComponent<LoadHandler>();
        if (NetworkManager.Singleton.StartHost() == false) 
        {
            GameObject.Destroy(GameObject.Find("LobbyHandler"));
            load_handler.displayLostConnection("Connection was lost.");
        };

        //wait for NetworkManager to load
        while (NetworkManager.Singleton.IsListening == false || GameObject.FindGameObjectWithTag("Player") == null)
        {
            yield return null;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<CameraMove>().GetCamera().SetActive(true);

        //wait a frame to let scripts initialize
        yield return null;

        placeShipAtStart();

        //enable all stations
        for (int i = 0; i < 4; i++)
        {
            ReferenceAssistor.Instance.power_manager.powerStation(i);
            ReferenceAssistor.Instance.module_handlers[4].GetComponent<PowerControl>().turnDial(i, true);
        }

        //set lights to white
        for (int i = 0; i < 4; i++)
        {
            ReferenceAssistor.Instance.lights_manager.changeSectionAppearance(i, ReferenceAssistor.Instance.lit_white, Color.white);
        }
        ReferenceAssistor.Instance.lights_manager.changeSectionAppearance(4, ReferenceAssistor.Instance.lit_white, new Color(0.5f, 0.5f, 0.5f));

        //load scenario objects
        List<TrainingDrone> training_drones = loadObstacles();

        yield return new WaitForSeconds(1.0f);

        load_handler.GetComponent<LoadHandler>().endLoad(true);
        ReferenceAssistor.Instance.player_manager.addPlayer(player, load_handler);
        player.transform.localPosition = PLAYER_STARTING_POSITION;
        PlayerManager.unfreezePlayer(player);
        PrimaryScript.Instance.unlockPlayer(player);
        ReferenceAssistor.Instance.audio_manager.InitializeAudio();
        ReferenceAssistor.Instance.audio_manager.ActivateComputerVoice();
        ReferenceAssistor.Instance.spaceship.GetComponent<ShipMovement>().SetPaths(-1.0f * ScenarioManager.getBoundaryPointFromAngle(0.0f), 0.0f, ScenarioManager.getBoundaryPointFromAngle(0.0f), 0.0f);
        ReferenceAssistor.Instance.spaceship.GetComponent<ShipMovement>().UnlockMovement();
        ReferenceAssistor.Instance.power_manager.GetComponent<PowerRegulator>().initializePowerRegulator();
        ReferenceAssistor.Instance.spaceship.GetComponent<ShipInventory>().initializeInventory();
        ReferenceAssistor.Instance.module_handlers[1].GetComponent<EncryptionKeys>().initializeEncryptionKeys();
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<EngineCoolantSupply>().initializeEngineTemperatureIncreaser();
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ComputerRegulator>().initializeComputerRegulator();
        ReferenceAssistor.Instance.module_handlers[4].GetComponent<PrefixCodeManager>().initiatePrefixCodeManager();
        ReferenceAssistor.Instance.module_handlers[4].GetComponent<CrewManifest>().reportAsReady();
        ReferenceAssistor.Instance.light_layer_two.gameObject.SetActive(true);

        foreach (TrainingDrone td in training_drones)
        {
            td.initializeDrone();
        }

        yield return new WaitForSeconds(1.0f);

        displaySimulationStart();
    }

    private void placeShipAtStart()
    {
        ReferenceAssistor.Instance.world_root.transform.position = new Vector3(0.0f, 0.0f, SHIP_SETBACK_DISTANCE);
        ReferenceAssistor.Instance.spaceship.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        ReferenceAssistor.Instance.module_handlers[0].GetComponent<FlyingInstruments>().updateAltimeterScreen(0.0f);
        ReferenceAssistor.Instance.module_handlers[0].GetComponent<FlyingInstruments>().updateCourseHeadingScreen(0.0f, "90.0°");
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ScenarioMap>().updateAltitude(0.0f);
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ScenarioMap>().updateShipLocation(ReferenceAssistor.Instance.world_root.transform.position);
    }

    public void endTraining(bool successful)
    {
        if (successful == true)
        {
            displaySimulationSuccess();
        }
        else
        {
            displaySimulationFailure();
        }
        simulation_number++;

        ReferenceAssistor.Instance.spaceship.GetComponent<ShipMovement>().LockMovement();
        ReferenceAssistor.Instance.spaceship.GetComponent<ShipHealth>().resetToFullHealth();
        ReferenceAssistor.Instance.power_manager.GetComponent<PowerRegulator>().useAuxiliaryPower();
        placeShipAtStart();

        List<TrainingDrone> training_drones = loadObstacles();
        foreach (TrainingDrone td in training_drones)
        {
            td.initializeDrone();
        }

        ReferenceAssistor.Instance.spaceship.GetComponent<ShipMovement>().UnlockMovement();
    }

    private List<TrainingDrone> loadObstacles()
    {
        //clear existing obstacles
        foreach (Transform t in ReferenceAssistor.Instance.world_root.transform)
        {
            GameObject.Destroy(t.gameObject);
        }

        //spawn new obstacles
        GameObject training_content = GameObject.Instantiate(training_obstacles, ReferenceAssistor.Instance.world_root.transform);
        training_content.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 359.9f), 0.0f);
        List<TrainingDrone> training_drones = new List<TrainingDrone>();
        foreach (Transform t in training_content.transform)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                GameObject o = t.GetChild(i).gameObject;
                o.GetComponent<NetworkObject>().Spawn(true);
                o.GetComponent<NetworkObject>().TrySetParent(ReferenceAssistor.Instance.world_root, true);
                TrainingDrone td = o.GetComponent<TrainingDrone>();
                if (td != null)
                {
                    training_drones.Add(td);
                }
            }
        }
        GameObject.Destroy(training_content);

        return training_drones;
    }

    private void changeSimulationColor(Color c)
    {
        foreach (Transform t in training_hud_display.transform.GetChild(0))
        {
            t.GetComponent<TMP_Text>().color = c;
        }
        c.a = 0.2f;
        training_hud_display.transform.GetChild(1).GetComponent<UnityEngine.UI.RawImage>().color = c;
    }

    private void updateSimulationAttemptsCounter()
    {
        training_hud_display.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().SetText("SIMULATION #" + simulation_number);
    }

    private void displaySimulationStart()
    {
        training_hud_display.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().SetText("SIMULATION ONLINE");
        changeSimulationColor(ReferenceAssistor.COLOR_OPTIONS[0]);
        updateSimulationAttemptsCounter();
        StartCoroutine(displaySimulationInfo());
    }

    private void displaySimulationSuccess()
    {
        training_hud_display.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().SetText("MISSION COMPLETED");
        changeSimulationColor(ReferenceAssistor.COLOR_OPTIONS[3]);
        updateSimulationAttemptsCounter();
        StartCoroutine(displaySimulationInfo());
    }

    private void displaySimulationFailure()
    {
        training_hud_display.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().SetText("SIMULATION FAILED");
        changeSimulationColor(Color.red);
        updateSimulationAttemptsCounter();
        StartCoroutine(displaySimulationInfo());
    }

    IEnumerator fadeHelper(CanvasGroup cg, float starting_a, float ending_a, float time)
    {
        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            cg.alpha = Mathf.Lerp(ending_a, starting_a, anim_time / time);

            yield return null;
        }
    }

    IEnumerator displaySimulationInfo()
    {
        StartCoroutine(fadeHelper(training_hud_display.transform.GetChild(0).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f));
        yield return StartCoroutine(fadeHelper(training_hud_display.transform.GetChild(1).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f));

        yield return new WaitForSeconds(3.0f);

        StartCoroutine(fadeHelper(training_hud_display.transform.GetChild(0).GetComponent<CanvasGroup>(), 1.0f, 0.0f, 1.0f));
        StartCoroutine(fadeHelper(training_hud_display.transform.GetChild(1).GetComponent<CanvasGroup>(), 1.0f, 0.0f, 1.0f));
    }
}