/*
    EndSequenceManager.cs
    - Used to handle things that pertain to the ending sequence of visiting Deep Space Five
    Contributor(s): Jake Schott
    Last Updated: 11/1/2026
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Steamworks;

public class EndSequenceManager : NetworkBehaviour
{
    //CLASS CONSTANTS
    private static float BRIDGE_DOORS_OPEN_TIME = 2.0f;
    private static float TURBO_ELEVATOR_DOORS_CLOSE_TIME = 0.5f;
    private static float TURBO_ELEVATOR_MOVE_TIME = 5.0f;

    public GameObject hallway;
    public GameObject bridge_doors;
    public GameObject turbo_elevator_doors;
    public GameObject turbo_elevator_control_panel_display;
    public AudioSource turbo_elevator_hum;
    public GameObject bridge;
    public GameObject exterior_points;
    private GameObject end_sequence_camera;
    private GameObject end_sequence_ship;
    private GameObject end_sequence_camera_positions;
    private GameObject end_sequence_ship_positions;

    private LobbyHandler lobby_handler;
    private bool turbo_elevator_activated = false;
    private Dictionary<ulong, bool> players_ready = new Dictionary<ulong, bool>(); //key steam ID, value ready or not

    private void Start()
    {
        lobby_handler = GameObject.Find("LobbyHandler").GetComponent<LobbyHandler>();
        foreach (ulong steam_id in lobby_handler.getPlayerSteamIDsInLobby())
        {
            if (steam_id != 0) //ignore empty slots
            {
                players_ready.Add(steam_id, false);
            }
        }
    }

    //returns true if all currently connected players are marked as ready
    private bool allPlayersReady()
    {
        List<ulong> player_steam_ids = lobby_handler.getPlayerSteamIDsInLobby();
        for (int i = 0; i < player_steam_ids.Count; i++)
        {
            if (player_steam_ids[i] != 0)
            {
                if (players_ready[player_steam_ids[i]] == false)
                {
                    return false;
                }
            }
        }
        return true;
    }

    //resets ready states of every player
    private void resetReadyPlayers()
    {
        List<ulong> player_steam_ids = lobby_handler.getPlayerSteamIDsInLobby();
        for (int i = 0; i < player_steam_ids.Count; i++)
        {
            if (players_ready.ContainsKey(player_steam_ids[i]) == true)
            {
                players_ready[player_steam_ids[i]] = false;
            }
        }
    }

    private GameObject unlockPlayerMovementHelper()
    {
        GameObject player = ReferenceAssistor.Instance.player_manager.getLocalPlayer();
        EndSequenceReferenceAssistor.Instance.EndSequenceCamera.SetActive(false);
        player.GetComponent<CameraMove>().ReactivateCamera();
        PrimaryScript.Instance.activate();
        ReferenceAssistor.Instance.audio_manager.UnmuteSFX();
        foreach (GameObject plr in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerManager.unfreezePlayer(plr);
        }
        return player;
    }

    //called by PlayerManager.endTransitionRPC()
    public void startEndSequence()
    {
        StartCoroutine(endSequenceIntro());
    }

    //initial camera shots of Deep Space Five and opening of the bridge doors
    IEnumerator endSequenceIntro()
    {
        //get references
        end_sequence_camera = EndSequenceReferenceAssistor.Instance.EndSequenceCamera;
        end_sequence_ship = EndSequenceReferenceAssistor.Instance.EndSequenceShip;
        end_sequence_camera_positions = EndSequenceReferenceAssistor.Instance.EndSequenceCameraPositions;
        end_sequence_ship_positions = EndSequenceReferenceAssistor.Instance.EndSequenceShipPositions;

        //switch cameras
        end_sequence_camera.SetActive(true);

        //prep the cutscene
        ReferenceAssistor.Instance.player_manager.freezeAllPlayers();
        ReferenceAssistor.Instance.player_manager.getLocalPlayer().transform.localScale = Vector3.zero;
        bridge.transform.localScale = Vector3.zero;
        exterior_points.transform.localScale = Vector3.zero;
        end_sequence_ship.SetActive(true);

        //initial ship fly-in
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(0), end_sequence_camera_positions.transform.GetChild(1), true, 15.0f);
        yield return new WaitForSeconds(3.0f);
        yield return ReferenceAssistor.Instance.translation_helper.linearTransation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(0), end_sequence_ship_positions.transform.GetChild(1), 12.0f);

        //next shot
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(1), end_sequence_ship_positions.transform.GetChild(2), false, 12.0f);
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(2), end_sequence_camera_positions.transform.GetChild(3), true, 11.0f);

        //next shot
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(4), end_sequence_camera_positions.transform.GetChild(5), false, 4.0f);

        //next shot
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(2), end_sequence_ship_positions.transform.GetChild(3), true, 12.0f);
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(6), end_sequence_camera_positions.transform.GetChild(7), true, 11.0f);

        //next shot
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(8), end_sequence_camera_positions.transform.GetChild(9), true, 7.0f);

        //next shot
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(10), end_sequence_camera_positions.transform.GetChild(11), true, 18.0f);

        //wait a second
        yield return new WaitForSeconds(1.0f);

        readyForNextSegmentRPC(SteamClient.SteamId);
        if (NetworkManager.Singleton.IsHost == true)
        {
            //wait until all players are done for next step
            while (allPlayersReady() == false)
            {
                yield return null;
            }

            resetReadyPlayers();
            activateBridgeDoorsOpenRPC();
        }
    }

    IEnumerator openBridgeDoors()
    {
        //wait a bit
        yield return new WaitForSeconds(1.5f);

        //open bridge door
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(bridge_doors.transform.GetChild(0), new Vector3(-2.4f, 0.0f, 0.0f), true, BRIDGE_DOORS_OPEN_TIME);
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(bridge_doors.transform.GetChild(1), new Vector3(2.4f, 0.0f, 0.0f), true, BRIDGE_DOORS_OPEN_TIME);
        bridge_doors.transform.GetChild(2).GetComponent<AudioSource>().Play();
    }

    IEnumerator turboElevatorSequence()
    {
        //close the turbo elevator doors
        turbo_elevator_doors.transform.GetChild(2).GetComponent<AudioSource>().Play();
        ReferenceAssistor.Instance.translation_helper.linearTranslation(turbo_elevator_doors.transform.GetChild(0), new Vector3(0.0f, 0.0f, 0.0f), TURBO_ELEVATOR_DOORS_CLOSE_TIME);
        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(turbo_elevator_doors.transform.GetChild(1), new Vector3(0.0f, 0.0f, 0.0f), TURBO_ELEVATOR_DOORS_CLOSE_TIME);

        //mute ship beeps
        ReferenceAssistor.Instance.audio_manager.FadeSound(ReferenceAssistor.Instance.audio_manager.ship_beeps, 0.0f, 0.5f);

        //hide bridge for performance
        bridge.gameObject.SetActive(false);
        foreach (GameObject mh in ReferenceAssistor.Instance.module_handlers)
        {
            mh.gameObject.SetActive(false);
        }
        ReferenceAssistor.Instance.background_animator.gameObject.SetActive(false);

        //fade in elevator sound
        turbo_elevator_hum.Play();
        ReferenceAssistor.Instance.audio_manager.FadeSound(turbo_elevator_hum, 0.1f, 1.0f);

        //update arrow on turbo elevator panel
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(turbo_elevator_control_panel_display.transform.GetChild(4).GetChild(0), new Vector3(-0.0025f, 0.0f, 0.0f), true, TURBO_ELEVATOR_MOVE_TIME);
        yield return new WaitForSeconds(TURBO_ELEVATOR_MOVE_TIME - 1.0f);

        //fade out elevator sound
        yield return ReferenceAssistor.Instance.audio_manager.FadeSound(turbo_elevator_hum, 0.0f, 1.0f);
        turbo_elevator_hum.Stop();

        //update floor marker on turbo elevator panel
        Color c = ReferenceAssistor.COLOR_OPTIONS[0];
        turbo_elevator_control_panel_display.transform.GetChild(2).GetComponent<UnityEngine.UI.RawImage>().color = c;
        c.a = 0.2f;
        turbo_elevator_control_panel_display.transform.GetChild(1).GetComponent<UnityEngine.UI.RawImage>().color = c;

        //wait a second
        yield return new WaitForSeconds(1.0f);

        readyForNextSegmentRPC(SteamClient.SteamId);
        if (NetworkManager.Singleton.IsHost == true)
        {
            //wait until all players are done for next step
            while (allPlayersReady() == false)
            {
                yield return null;
            }

            //reparent players to DSF players holder
            foreach (GameObject plr in GameObject.FindGameObjectsWithTag("Player"))
            {
                plr.GetComponent<NetworkObject>().TrySetParent(EndSequenceReferenceAssistor.Instance.DeepSpaceFivePlayersHolder, true);
            }
            resetReadyPlayers();
            activateDeepSpaceFiveEntryRPC();
        }
    }

    IEnumerator deepSpaceFiveEntrySequence()
    {
        //switch cameras
        GameObject player = ReferenceAssistor.Instance.player_manager.getLocalPlayer();
        PrimaryScript.Instance.deactivate(true, false);
        player.GetComponent<CameraMove>().DeactivateCamera();
        end_sequence_camera.SetActive(true);
        ReferenceAssistor.Instance.audio_manager.MuteSFX();

        //position player
        ReferenceAssistor.Instance.player_manager.freezeAllPlayers();
        int spawn_index = lobby_handler.getPlayerIndex(SteamClient.SteamId);
        player.transform.position = EndSequenceReferenceAssistor.Instance.DeepSpaceFiveSpawnPoints.transform.GetChild(spawn_index).position;
        foreach (GameObject plr in GameObject.FindGameObjectsWithTag("Player"))
        {
            plr.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        }

        //hide hallway for performance
        hallway.SetActive(false);

        //set ship
        end_sequence_ship.SetActive(true);
        end_sequence_ship.transform.position = Vector3.zero;
        end_sequence_ship.transform.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

        //animate camera
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_camera_positions.transform.GetChild(12), end_sequence_camera_positions.transform.GetChild(13), true, 15.0f);

        yield return new WaitForSeconds(1.0f);

        readyForNextSegmentRPC(SteamClient.SteamId);
        if (NetworkManager.Singleton.IsHost == true)
        {
            //wait until all players are done for next step
            while (allPlayersReady() == false)
            {
                yield return null;
            }
            resetReadyPlayers();
            activateDeepSpaceFiveFreeRoamRPC();
        }
    }

    //called when everyone has loaded the end sequence scene
    [Rpc(SendTo.Everyone)]
    public void prepEndSequenceRPC()
    {
        if (NetworkManager.Singleton.IsHost == true)
        {
            ReferenceAssistor.Instance.scenario_manager.powerAllStationsRPC();
        }
        ((BridgePrimaryScript)PrimaryScript.Instance).enableStandingModeOnly();
        PrimaryScript.Instance.GetComponent<SecondaryScript>().setSecondaryInfoVisibility(false);
        PrimaryScript.Instance.GetComponent<SecondaryScript>().setPermanentOverlayTopAccessible(false);
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ScenarioCountdown>().hideCountdown();
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ScenarioNavigation>().displayEndSequence();
    }

    //called when loaded current segment
    [Rpc(SendTo.Everyone)]
    private void readyForNextSegmentRPC(ulong steam_id)
    {
        if (NetworkManager.Singleton.IsHost == true)
        {
            players_ready[steam_id] = true;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void activateBridgeDoorsOpenRPC()
    {
        //unlock player movement
        end_sequence_ship.SetActive(false);
        bridge.transform.localScale = Vector3.one;
        hallway.SetActive(true);
        ReferenceAssistor.Instance.player_manager.getLocalPlayer().transform.localScale = Vector3.one;
        unlockPlayerMovementHelper();

        //open bridge doors
        StartCoroutine(openBridgeDoors());
    }

    //called when all players are in the turbo elevator
    [Rpc(SendTo.Everyone)]
    public void activateTurboElevatorRPC()
    {
        if (turbo_elevator_activated == true)
        {
            return;
        }
        turbo_elevator_activated = true;
        StartCoroutine(turboElevatorSequence());
    }

    //called when it's time to move into Deep Space Five
    [Rpc(SendTo.Everyone)]
    private void activateDeepSpaceFiveEntryRPC()
    {
        StartCoroutine(deepSpaceFiveEntrySequence());
    }

    //called when all players have finished the quick camera transition from elevator to DSF
    [Rpc(SendTo.Everyone)]
    private void activateDeepSpaceFiveFreeRoamRPC()
    {
        //unlock player movement
        unlockPlayerMovementHelper().GetComponent<CameraMove>().UnlockCamera(new Vector2(90.0f, 0.0f));
        ReferenceAssistor.Instance.audio_manager.MuteShipBeeps();
        PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupNotification(1);

        //open DSF door
        ReferenceAssistor.Instance.translation_helper.linearTranslation(EndSequenceReferenceAssistor.Instance.DeepSpaceFiveGateBDoor.transform, new Vector3(0.0f, 0.0f, -5.6f), 12.0f);
    }
}