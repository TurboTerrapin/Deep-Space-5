/*
    EndSequenceManager.cs
    - Used to handle things that pertain to the ending sequence of visiting Deep Space Five
    Contributor(s): Jake Schott
    Last Updated: 11/25/2026
*/

using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class EndSequenceManager : NetworkBehaviour, IInteractable
{
    //CLASS CONSTANTS
    private static float BRIDGE_DOORS_OPEN_TIME = 2.0f;
    private static float TURBO_ELEVATOR_DOORS_CLOSE_TIME = 0.5f;
    private static float TURBO_ELEVATOR_MOVE_TIME = 5.0f;
    public static Color[] DEEP_SPACE_FIVE_INTERACTION_COLORS = new Color[] { ReferenceAssistor.COLOR_OPTIONS[4], ReferenceAssistor.COLOR_OPTIONS[4] };
    private List<string> DEEP_SPACE_FIVE_INTERACTION_NAMES = new List<string>() { "PART INVENTORY", "COMPUTER INTERFACE", "WARP CORE" };
    private static float INTERACTION_RADIUS = 0.5f;

    public GameObject hallway;
    public GameObject bridge_doors;
    public GameObject turbo_elevator_doors;
    public GameObject turbo_elevator_control_panel_display;
    public AudioSource turbo_elevator_hum;
    public GameObject bridge;
    public GameObject exterior_points;
    public GameObject endgame_canvas;
    private GameObject end_sequence_camera;
    private GameObject end_sequence_ship;
    private GameObject end_sequence_arrival_camera_positions;
    private GameObject end_sequence_interaction_camera_positions;
    private GameObject end_sequence_departure_camera_positions;
    private GameObject end_sequence_ship_positions;
    private Material[] ship_materials;
    private Light[] ship_engine_lights = new Light[5];
    private UnityEngine.Rendering.Universal.ScreenSpaceLensFlare screen_space_lens_flare;
    private UnityEngine.Rendering.Universal.LensDistortion lens_distortion;
    private LobbyHandler lobby_handler;

    private bool ignore_lobby_disconnect = false;
    private bool[] deep_space_five_interactions_complete = new bool[3] { false, false, false };
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

    //returns true if in the final cutscene
    public bool shouldIgnoreLobbyDisconnect()
    {
        return ignore_lobby_disconnect;
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

    //unfreezes all players and switches to player camera
    private GameObject unlockPlayerMovement()
    {
        GameObject player = ReferenceAssistor.Instance.player_manager.getLocalPlayer();
        EndSequenceReferenceAssistor.Instance.end_sequence_camera.SetActive(false);
        player.GetComponent<CameraMove>().ReactivateCamera();
        player.GetComponent<CameraMove>().UpdateFacialFeaturesVisibility(!CameraMove.HIDE_HAIR_AND_EYES);
        PrimaryScript.Instance.activate();
        ReferenceAssistor.Instance.audio_manager.UnmuteSFX();
        foreach (GameObject plr in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerManager.unfreezePlayer(plr);
        }
        return player;
    }

    //freezes all players and switches to the cutscene camera
    private void lockPlayerMovement()
    {
        GameObject player = ReferenceAssistor.Instance.player_manager.getLocalPlayer();
        player.GetComponent<CameraMove>().UpdateFacialFeaturesVisibility(true);
        PrimaryScript.Instance.deactivate(true, false);
        player.GetComponent<CameraMove>().DeactivateCamera();
        end_sequence_camera.SetActive(true);
        ReferenceAssistor.Instance.player_manager.freezeAllPlayers();
    }

    //called by PlayerManager.endTransitionRPC()
    public void startEndSequence()
    {
        StartCoroutine(endSequenceIntro());
    }

    //called by PrimaryScript.checkForInteractables()
    public void activateInteraction(string interaction_name)
    {
        int index = DEEP_SPACE_FIVE_INTERACTION_NAMES.IndexOf(interaction_name);
        if (deep_space_five_interactions_complete[index] == false)
        {
            activateDeepSpaceFiveInteractionRPC(index, SteamClient.SteamId, CustomizeCharacterMenu.GetFirstInitialAndLastName(), CustomizeCharacterMenu.GetSelectedClothingOption());
        }
    }

    //called by BridgePrimaryScript.getClosestInteractable()
    public int getClosestInteractable(Vector3 player_pos)
    {
        int closest_interactable = -1;
        float closest_dist = 9999.9f;

        for (int i = 0; i < deep_space_five_interactions_complete.Length; i++)
        {
            if (deep_space_five_interactions_complete[i] == false)
            {
                float distance_to_interaction_point = Vector3.Distance(player_pos, EndSequenceReferenceAssistor.Instance.deep_space_five_interaction_points.transform.GetChild(i).position);
                if (distance_to_interaction_point < INTERACTION_RADIUS && distance_to_interaction_point < closest_dist)
                {
                    closest_interactable = i;
                }
            }
        }

        return closest_interactable;
    }

    //initial camera shots of Deep Space Five and opening of the bridge doors
    IEnumerator endSequenceIntro()
    {
        //switch cameras
        end_sequence_camera.SetActive(true);

        //prep the cutscene
        ReferenceAssistor.Instance.player_manager.freezeAllPlayers();
        ReferenceAssistor.Instance.player_manager.getLocalPlayer().transform.localScale = Vector3.zero;
        bridge.transform.localScale = Vector3.zero;
        ReferenceAssistor.Instance.lights_manager.default_light_group.SetActive(false);
        exterior_points.transform.localScale = Vector3.zero;
        end_sequence_ship.SetActive(true);

        //initial ship fly-in
        EndSequenceReferenceAssistor.Instance.end_sequence_cutscene_sounds.transform.GetChild(0).GetComponent<AudioSource>().Play();
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(0), end_sequence_arrival_camera_positions.transform.GetChild(1), true, 16.0f);
        yield return new WaitForSeconds(4.0f);

        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(0), end_sequence_ship_positions.transform.GetChild(1), 12.0f);

        //next shot
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(1), end_sequence_ship_positions.transform.GetChild(2), false, 9.0f);
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(2), end_sequence_arrival_camera_positions.transform.GetChild(3), true, 8.0f);

        //next shot
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(4), end_sequence_arrival_camera_positions.transform.GetChild(5), false, 3.0f);

        //next shot
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(2), end_sequence_ship_positions.transform.GetChild(3), true, 10.0f);
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(6), end_sequence_arrival_camera_positions.transform.GetChild(7), true, 9.0f);

        //next shot
        EndSequenceReferenceAssistor.Instance.deep_space_five_interior.SetActive(true);
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(8), end_sequence_arrival_camera_positions.transform.GetChild(9), true, 5.0f);

        //next shot
        EndSequenceReferenceAssistor.Instance.deep_space_five_interior.SetActive(false);
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(10), end_sequence_arrival_camera_positions.transform.GetChild(11), true, 12.0f);

        //wait a second
        yield return new WaitForSeconds(0.5f);

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

    //handles the delayed opening of the bridge doors after the initial cutscene
    IEnumerator openBridgeDoors()
    {
        //wait a bit
        yield return new WaitForSeconds(1.5f);

        //open bridge door
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(bridge_doors.transform.GetChild(0), new Vector3(-2.4f, 0.0f, 0.0f), true, BRIDGE_DOORS_OPEN_TIME);
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(bridge_doors.transform.GetChild(1), new Vector3(2.4f, 0.0f, 0.0f), true, BRIDGE_DOORS_OPEN_TIME);
        bridge_doors.transform.GetChild(2).GetComponent<AudioSource>().Play();
    }

    //handles moving the turbo elevator down
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
                plr.GetComponent<NetworkObject>().TrySetParent(EndSequenceReferenceAssistor.Instance.deep_space_five_players_holder, true);
            }
            resetReadyPlayers();
            List<string> players_who_made_it_to_deep_space_five = new List<string>();
            foreach (KeyValuePair<ulong, bool> kvp in players_ready)
            {
                players_who_made_it_to_deep_space_five.Add(ReferenceAssistor.Instance.module_handlers[4].GetComponent<CrewManifest>().getPlayerFirstInitialAndLastName(kvp.Key));
            }
            int empty_slots = 4 - players_who_made_it_to_deep_space_five.Count;
            for (int i = 0; i < empty_slots; i++)
            {
                players_who_made_it_to_deep_space_five.Add("");
            }
            activateDeepSpaceFiveEntryRPC(players_who_made_it_to_deep_space_five[0], players_who_made_it_to_deep_space_five[1], players_who_made_it_to_deep_space_five[2], players_who_made_it_to_deep_space_five[3]);
        }
    }

    //handles quick camera movement that does a flip around and plays the short song
    IEnumerator deepSpaceFiveEntrySequence()
    {
        //switch cameras
        GameObject player = ReferenceAssistor.Instance.player_manager.getLocalPlayer();
        PrimaryScript.Instance.deactivate(true, false);
        player.GetComponent<CameraMove>().UnlockCamera(new Vector2(90.0f, 0.0f));
        player.GetComponent<CameraMove>().DeactivateCamera();
        player.GetComponent<CameraMove>().UpdateFacialFeaturesVisibility(true);
        end_sequence_camera.SetActive(true);

        //position player
        ReferenceAssistor.Instance.player_manager.freezeAllPlayers();
        int spawn_index = lobby_handler.getPlayerIndex(SteamClient.SteamId);
        player.transform.position = EndSequenceReferenceAssistor.Instance.deep_space_five_spawn_points.transform.GetChild(spawn_index).position;
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

        //show DSF interior
        EndSequenceReferenceAssistor.Instance.deep_space_five_interior.SetActive(true);

        //animate camera
        EndSequenceReferenceAssistor.Instance.end_sequence_cutscene_sounds.transform.GetChild(1).GetComponent<AudioSource>().Play();
        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_arrival_camera_positions.transform.GetChild(12), end_sequence_arrival_camera_positions.transform.GetChild(13), true, 10.0f);

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

    //places interacting player at the corresponding animation point for the two computer interaction cutscenes
    private void placeInteractingPlayer(ulong steam_id, int animation_index)
    {
        GameObject interacting_player = ReferenceAssistor.Instance.player_manager.getPlayer(steam_id);
        if (interacting_player != null)
        {
            Transform player_point = EndSequenceReferenceAssistor.Instance.deep_space_five_animation_points.transform.GetChild(animation_index);
            if (interacting_player == ReferenceAssistor.Instance.player_manager.getLocalPlayer())
            {
                interacting_player.GetComponent<CameraMove>().UnlockCamera(new Vector2(player_point.localEulerAngles.x - 90.0f, 0.0f));
            }
            interacting_player.transform.position = player_point.position;
            interacting_player.transform.rotation = player_point.rotation;
        }
    }

    //handles the computer entry for retrieving the warp core
    IEnumerator warpCoreLookup()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject selector_display = EndSequenceReferenceAssistor.Instance.part_inventory_selector_display;
        string part_name = "WARP CORE";
        int iterator = 0;
        TMP_Text search_field = selector_display.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>();
        while (iterator < part_name.Length)
        {
            iterator++;
            search_field.SetText(part_name.Substring(0, iterator));
            EndSequenceReferenceAssistor.Instance.part_inventory_key_boop_sound.GetComponent<AudioSource>().Play();

            yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
        }

        selector_display.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().enabled = false;
        selector_display.transform.GetChild(1).GetComponent<CanvasGroup>().alpha = 1.0f;
        selector_display.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        selector_display.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText(part_name);
        selector_display.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().SetText("PART NUMBER: 5099");
        selector_display.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().SetText("LOADABLE: <color=#00FF00>YES</color>");
        selector_display.transform.GetChild(1).GetChild(4).GetChild(0).GetChild(1).GetComponent<TMP_Text>().SetText("3");
        selector_display.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
    }

    //handles the progress bar for retrieving the warp core
    IEnumerator warpCoreLoad()
    {
        GameObject progress_display = EndSequenceReferenceAssistor.Instance.part_inventory_progress_display;
        GameObject inventory_doors = EndSequenceReferenceAssistor.Instance.part_inventory_doors;

        progress_display.transform.GetChild(2).GetComponent<CanvasGroup>().alpha = 1.0f;
        UnityEngine.UI.Image progress_bar = progress_display.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>();
        EndSequenceReferenceAssistor.Instance.part_inventory_progress_sound.GetComponent<AudioSource>().Play();

        float anim_time = 3.0f;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);
            progress_bar.fillAmount = 1.0f - (anim_time / 3.0f);

            yield return null;
        }

        progress_display.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 1.0f;
        progress_display.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        progress_display.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);

        inventory_doors.transform.GetChild(2).GetComponent<AudioSource>().Play();
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(inventory_doors.transform.GetChild(0), new Vector3(0.0f, 0.0f, 0.42f), true, 1.0f);
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(inventory_doors.transform.GetChild(1), new Vector3(0.0f, 0.0f, -0.42f), true, 1.0f);
    }

    //handles the part inventory sequence with the opening doors
    IEnumerator warpCoreRetrievalSequence(ulong interacting_steam_id, string interacting_player_name, Color dialogue_color)
    {
        placeInteractingPlayer(interacting_steam_id, 0);
        lockPlayerMovement();

        PrimaryScript.Instance.GetComponent<SecondaryScript>().clearPopupNotifications();
        Coroutine c = ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(0), end_sequence_interaction_camera_positions.transform.GetChild(1), 8.0f);
        yield return new WaitForSeconds(1.0f);

        PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupDialogue(interacting_player_name, "Let's see if they have a replacement warp core in stock.", dialogue_color, 3.0f);
        yield return c;

        StartCoroutine(warpCoreLookup());
        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(2), end_sequence_interaction_camera_positions.transform.GetChild(3), 5.0f);

        StartCoroutine(warpCoreLoad());
        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(4), end_sequence_interaction_camera_positions.transform.GetChild(5), 5.0f);

        PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupDialogue(interacting_player_name, "Bingo.", dialogue_color, 3.0f);
        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(6), end_sequence_interaction_camera_positions.transform.GetChild(7), 6.0f);

        readyForNextSegmentRPC(SteamClient.SteamId);
        if (NetworkManager.Singleton.IsHost == true)
        {
            //wait until all players are done for next step
            while (allPlayersReady() == false)
            {
                yield return null;
            }
            resetReadyPlayers();
            interactionCompleteRPC(canReturnToShip());
        }
    }

    //handles the computer interface sequence with the retractable screen
    IEnumerator messageBroadcastSequence(ulong interacting_steam_id, string interacting_player_name, Color dialogue_color)
    {
        placeInteractingPlayer(interacting_steam_id, 1);
        lockPlayerMovement();

        PrimaryScript.Instance.GetComponent<SecondaryScript>().clearPopupNotifications();
        Coroutine c = ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(8), end_sequence_interaction_camera_positions.transform.GetChild(9), 9.0f);
        yield return new WaitForSeconds(1.0f);

        PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupDialogue(interacting_player_name, "I think it's time we set the record straight, once and for all.", dialogue_color, 4.0f);
        yield return c;

        c = ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(10), end_sequence_interaction_camera_positions.transform.GetChild(11), 9.0f);
        yield return new WaitForSeconds(1.0f);

        GameObject computer_interface_display = EndSequenceReferenceAssistor.Instance.computer_interface_display;
        AudioSource computer_interface_boop_sound = EndSequenceReferenceAssistor.Instance.computer_interface_boop_sound.GetComponent<AudioSource>();

        computer_interface_display.transform.GetChild(0).GetChild(3).GetComponent<CanvasGroup>().alpha = 1.0f;
        computer_interface_display.transform.GetChild(0).GetChild(4).GetComponent<CanvasGroup>().alpha = 0.2f;
        computer_interface_boop_sound.Play();
        yield return new WaitForSeconds(0.25f);

        computer_interface_display.transform.GetChild(0).GetChild(2).GetComponent<CanvasGroup>().alpha = 1.0f;
        computer_interface_display.transform.GetChild(0).GetChild(3).GetComponent<CanvasGroup>().alpha = 0.2f;
        computer_interface_boop_sound.Play();
        yield return new WaitForSeconds(0.4f);

        computer_interface_display.transform.GetChild(0).gameObject.SetActive(false);
        computer_interface_display.transform.GetChild(1).gameObject.SetActive(true);
        computer_interface_boop_sound.Play();
        yield return new WaitForSeconds(0.5f);

        computer_interface_display.transform.GetChild(1).GetChild(0).GetComponent<CanvasGroup>().alpha = 0.2f;
        computer_interface_display.transform.GetChild(1).GetChild(1).GetComponent<CanvasGroup>().alpha = 1.0f;
        computer_interface_boop_sound.Play();
        yield return new WaitForSeconds(0.4f);

        computer_interface_display.transform.GetChild(1).gameObject.SetActive(false);
        computer_interface_display.transform.GetChild(2).gameObject.SetActive(true);
        computer_interface_boop_sound.Play();

        yield return new WaitForSeconds(0.5f);

        string message_chunk = "TO THE SCC LEADERSHIP COUNCIL,";
        int iterator = 0;
        TMP_Text message_field = computer_interface_display.transform.GetChild(2).GetChild(0).GetChild(1).GetComponent<TMP_Text>();
        while (iterator < message_chunk.Length)
        {
            iterator++;
            message_field.SetText(message_chunk.Substring(0, iterator));
            computer_interface_boop_sound.Play();

            yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
        }
        yield return c;

        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(EndSequenceReferenceAssistor.Instance.retractable_screen.transform, new Vector3(0.0f, 0.0f, 4.0f), true, 12.0f);
        EndSequenceReferenceAssistor.Instance.retractable_screen.GetComponent<AudioSource>().Play();
        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(12), end_sequence_interaction_camera_positions.transform.GetChild(13), 21.0f);

        c = ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_interaction_camera_positions.transform.GetChild(14), end_sequence_interaction_camera_positions.transform.GetChild(15), 6.5f);
        yield return new WaitForSeconds(0.5f);

        string dialogue = "Now let's grab that warp core and get out of here.";
        if (deep_space_five_interactions_complete[0] == true && deep_space_five_interactions_complete[1] == true)
        {
            dialogue = "Let's get out of here before anyone else shows up.";
        }
        PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupDialogue(interacting_player_name, dialogue, dialogue_color, 3.0f);
        yield return c;

        computer_interface_display.transform.GetChild(0).gameObject.SetActive(true);
        computer_interface_display.transform.GetChild(2).gameObject.SetActive(false);
        readyForNextSegmentRPC(SteamClient.SteamId);
        if (NetworkManager.Singleton.IsHost == true)
        {
            //wait until all players are done for next step
            while (allPlayersReady() == false)
            {
                yield return null;
            }
            resetReadyPlayers();
            interactionCompleteRPC(canReturnToShip());
        }
    }

    //handles gradually elongating the ship, increasing light intensity, and increasing camera effects, light intensity
    IEnumerator warpVisualization()
    {
        Vector2 engine_light_intensity_range = new Vector2(1000.0f, 300000.0f);
        Vector2 spin_factor_range = new Vector2(1.0f, 100.0f);
        Vector2 screen_space_lens_flare_intensity_range = new Vector2(0.0f, 0.1f);
        Vector2 lens_distortion_intensity_range = new Vector2(0.0f, -0.75f);
        Vector3 ship_scale_adjustment = new Vector3(1.0f, 1.1f, 1.0f);

        ShipExteriorFeatures ship_exterior_features = end_sequence_ship.GetComponent<ShipExteriorFeatures>();
        for (int i = 0; i < 5; i++)
        {
            ship_engine_lights[i] = EndSequenceReferenceAssistor.Instance.end_sequence_ship_engine_lights[i].GetComponent<Light>();
        }
        ReferenceAssistor.Instance.camera_settings.GetComponent<Volume>().profile.TryGet(out screen_space_lens_flare);
        ReferenceAssistor.Instance.camera_settings.GetComponent<Volume>().profile.TryGet(out lens_distortion);

        screen_space_lens_flare.active = true;
        screen_space_lens_flare.tintColor.Override(ReferenceAssistor.COLOR_OPTIONS[0]);
        screen_space_lens_flare.samples.Override(1);
        screen_space_lens_flare.vignetteEffect.Override(1.0f);
        screen_space_lens_flare.streaksIntensity.Override(0.0f);
        lens_distortion.active = true;
        lens_distortion.intensity.Override(0.0f);

        float anim_time = 13.0f;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float animation_progress = 1.0f - (anim_time / 13.0f);
            ship_exterior_features.adjustEngineCirclesRotationFactor(Mathf.Lerp(spin_factor_range.x, spin_factor_range.y, animation_progress));
            float engine_light_intensity = Mathf.Lerp(engine_light_intensity_range.x, engine_light_intensity_range.y, animation_progress);
            foreach (Light light in ship_engine_lights)
            {
                light.intensity = engine_light_intensity;
            }
            screen_space_lens_flare.intensity.Override(Mathf.Lerp(screen_space_lens_flare_intensity_range.x, screen_space_lens_flare_intensity_range.y, animation_progress));
            lens_distortion.intensity.Override(Mathf.Lerp(lens_distortion_intensity_range.x, lens_distortion_intensity_range.y, animation_progress));
            end_sequence_ship.transform.localScale = Vector3.Lerp(Vector3.one, ship_scale_adjustment, animation_progress);

            yield return null;
        }
    }

    //handles moving the ship forward, shrinking the ship, reducing light intensity, and reducing camera effects
    IEnumerator warpJump()
    {
        Vector2 engine_light_intensity_range = new Vector2(300000.0f, 0.0f);
        Vector2 screen_space_lens_flare_intensity_range = new Vector2(0.1f, 0.0f);
        Vector2 lens_distortion_intensity_range = new Vector2(-0.75f, 0.0f);
        Vector3 starting_ship_scale = new Vector3(1.0f, 1.1f, 1.0f);

        StartCoroutine(warpPortal());

        float anim_time = 1.0f;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float animation_progress = 1.0f - (anim_time / 1.0f);
            end_sequence_ship.transform.position = Vector3.Lerp(new Vector3(-40.0f, 0.0f, 400.0f), new Vector3(-40.0f, 0.0f, 4000.0f),  animation_progress);
            lens_distortion.intensity.Override(Mathf.Lerp(lens_distortion_intensity_range.x, lens_distortion_intensity_range.y, animation_progress));
            screen_space_lens_flare.intensity.Override(Mathf.Lerp(screen_space_lens_flare_intensity_range.x, screen_space_lens_flare_intensity_range.y, animation_progress));
            float engine_light_intensity = Mathf.Lerp(engine_light_intensity_range.x, engine_light_intensity_range.y, animation_progress);
            foreach (Light light in ship_engine_lights)
            {
                light.intensity = engine_light_intensity;
            }
            end_sequence_ship.transform.localScale = Vector3.Lerp(starting_ship_scale, Vector3.zero, animation_progress);

            yield return null;
        }

        end_sequence_ship.SetActive(false);
    }

    //handles the sprite renderer 2D warp portal that expands and contracts
    IEnumerator warpPortal()
    {
        GameObject warp_portal = EndSequenceReferenceAssistor.Instance.warp_portal;
        warp_portal.SetActive(true);
        SpriteRenderer main_background = warp_portal.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer center_background = warp_portal.transform.GetChild(1).GetComponent<SpriteRenderer>();
        GameObject rotating_cross = warp_portal.transform.GetChild(2).gameObject;
        SpriteRenderer oval_a = rotating_cross.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer oval_b = rotating_cross.transform.GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer cross_center = rotating_cross.transform.GetChild(2).GetComponent<SpriteRenderer>();
        SpriteRenderer[] sprite_renderers = new SpriteRenderer[] { main_background, center_background, oval_a, oval_b, cross_center };
        Color[] sprite_colors = new Color[] { main_background.color, center_background.color, oval_a.color, oval_b.color, cross_center.color };
        Vector3[] sprite_sizes = new Vector3[] { sprite_renderers[0].transform.localScale, sprite_renderers[1].transform.localScale, sprite_renderers[2].transform.localScale, sprite_renderers[3].transform.localScale, sprite_renderers[4].transform.localScale };

        foreach (SpriteRenderer sr in sprite_renderers)
        {
            sr.transform.localScale = Vector3.zero;
        }

        for (int i = 0; i < 2; i++)
        {
            float anim_time = 0.5f;
            while (anim_time > 0.0f)
            {
                anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

                float animation_progress = 1.0f - (anim_time / 0.5f);
                if (i == 1)
                {
                    animation_progress = 1.0f - animation_progress;
                }
                
                rotating_cross.transform.Rotate(new Vector3(0.0f, 0.0f, 300.0f * Time.deltaTime), Space.World);
                for (int k = 0; k < sprite_renderers.Length; k++)
                {
                    Color c = sprite_colors[k];
                    c.a = Mathf.Lerp(0.0f, sprite_colors[k].a, animation_progress);
                    sprite_renderers[k].color = c;
                    sprite_renderers[k].transform.localScale = Vector3.Lerp(Vector3.zero, sprite_sizes[k], animation_progress);
                }

                yield return null;
            }
        }
    }

    //used to fade canvas groups (mostly text)
    IEnumerator fadeGroup(CanvasGroup canvas_group, float starting_a, float ending_a, float time)
    {
        canvas_group.gameObject.SetActive(true);

        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            canvas_group.alpha = Mathf.Lerp(ending_a, starting_a, anim_time / time);

            yield return null;
        }
    }

    //used as a helper in the lights turn-on sequence
    private void switchShipWhiteLightsMaterial(Material to_switch_to)
    {
        ship_materials[9] = to_switch_to;
        end_sequence_ship.GetComponent<MeshRenderer>().materials = ship_materials;
    }

    //handles the final cutscene with the ship turning on and going to warp
    IEnumerator finalCutscene()
    {
        //show bar drinks, broken warp core, mute piano music, play final music
        EndSequenceReferenceAssistor.Instance.end_sequence_cutscene_sounds.transform.GetChild(2).GetComponent<AudioSource>().Play();
        EndSequenceReferenceAssistor.Instance.deep_space_five_interior.GetComponent<EndSequenceInteriorFeatures>().now_boarding_sound.SetActive(false);
        EndSequenceReferenceAssistor.Instance.deep_space_five_speaker_music.SetActive(false);
        EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_decorations.SetActive(true);
        foreach (GameObject light in EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_lights)
        {
            light.SetActive(false);
        }

        //turn lights on in sequence
        int[][] lights_to_turn_on = new int[][] { new int[] { 0 }, new int[] { 1 }, new int[] { 2, 3, 4 }, new int[] { 5 }, new int[] { 6 }  };
        Coroutine c;
        for (int i = 0; i < 5; i++)
        {
            switchShipWhiteLightsMaterial(ReferenceAssistor.Instance.pure_black);
            c = ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(i * 2), end_sequence_departure_camera_positions.transform.GetChild((i * 2) + 1), 1.25f);
            yield return new WaitForSeconds(0.5f);
            foreach (int k in lights_to_turn_on[i])
            {
                EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_lights[k].SetActive(true);
            }
            EndSequenceReferenceAssistor.Instance.end_sequence_cutscene_sounds.transform.GetChild(3).GetComponent<AudioSource>().Play();
            switchShipWhiteLightsMaterial(ReferenceAssistor.Instance.lit_white);
            EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_lights[0].gameObject.SetActive(true);
            yield return c;
        }

        c = ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(3), end_sequence_ship_positions.transform.GetChild(4), true, 10.0f);
        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(10), end_sequence_departure_camera_positions.transform.GetChild(11), 3.0f);

        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(12), end_sequence_departure_camera_positions.transform.GetChild(13), true, 5.0f);

        Coroutine d = ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(14), end_sequence_departure_camera_positions.transform.GetChild(15), true, 10.0f);
        yield return c;
        
        ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_ship.transform, end_sequence_ship_positions.transform.GetChild(4), end_sequence_ship_positions.transform.GetChild(5), true, 13.0f);
        yield return d;

        yield return ReferenceAssistor.Instance.translation_helper.exponentialTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(16), end_sequence_departure_camera_positions.transform.GetChild(17), true, 7.5f);

        EndSequenceReferenceAssistor.Instance.end_sequence_cutscene_sounds.transform.GetChild(4).GetComponent<AudioSource>().Play();
        end_sequence_camera.GetComponent<Camera>().farClipPlane = 5000.0f;
        c = StartCoroutine(warpVisualization());

        yield return new WaitForSeconds(2.0f);

        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(18), end_sequence_departure_camera_positions.transform.GetChild(19), 4.0f);

        yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(20), end_sequence_departure_camera_positions.transform.GetChild(21), 4.0f);

        d = ReferenceAssistor.Instance.translation_helper.linearTranslation(end_sequence_camera.transform, end_sequence_departure_camera_positions.transform.GetChild(22), end_sequence_departure_camera_positions.transform.GetChild(23), 11.0f);

        yield return c;

        yield return StartCoroutine(warpJump());

        CanvasGroup adventures_continue_group = endgame_canvas.transform.GetChild(1).GetChild(0).GetComponent<CanvasGroup>();
        yield return StartCoroutine(fadeGroup(adventures_continue_group, 0.0f, 1.0f, 1.0f));

        yield return new WaitForSeconds(4.0f);

        yield return StartCoroutine(fadeGroup(adventures_continue_group, 1.0f, 0.0f, 1.0f));

        yield return d;

        StartCoroutine(endCredits());
    }

    //called after going to warp
    IEnumerator endCredits()
    {
        end_sequence_camera.transform.position = end_sequence_departure_camera_positions.transform.GetChild(24).position;
        end_sequence_camera.transform.rotation = end_sequence_departure_camera_positions.transform.GetChild(24).rotation;

        CanvasGroup credit_group = endgame_canvas.transform.GetChild(1).GetChild(1).GetComponent<CanvasGroup>();
        TMP_Text position_title = endgame_canvas.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        TMP_Text credit_name = endgame_canvas.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<TMP_Text>();

        string[] position_titles = new string[4] { "DIRECTED BY", "PRODUCED BY", "PRODUCED BY", "PRODUCED BY" };
        string[] credit_names = new string[4] { "JAKE SCHOTT", "JOHN AYLWARD", "BEATA MUSIAL", "HENRYK MUSIAL" };
        for (int i = 0; i < 4; i++)
        {
            position_title.SetText(position_titles[i]);
            credit_name.SetText(credit_names[i]);
            yield return StartCoroutine(fadeGroup(credit_group, 0.0f, 1.0f, 0.5f));
            yield return new WaitForSeconds(2.5f);
            yield return StartCoroutine(fadeGroup(credit_group, 1.0f, 0.0f, 0.5f));
            yield return new WaitForSeconds(0.5f);
        }

        CanvasGroup black_screen_group = endgame_canvas.transform.GetChild(1).GetChild(2).GetComponent<CanvasGroup>();
        yield return StartCoroutine(fadeGroup(black_screen_group, 0.0f, 1.0f, 3.0f));

        yield return new WaitForSeconds(1.0f);
        PlayerManager.leaveGame();
    }

    //called when everyone has loaded the end sequence scene
    [Rpc(SendTo.Everyone)]
    public void prepEndSequenceRPC()
    {
        //prepare bridge and lock players from ship control
        if (NetworkManager.Singleton.IsHost == true)
        {
            ReferenceAssistor.Instance.scenario_manager.powerAllStationsRPC();
        }
        ((BridgePrimaryScript)PrimaryScript.Instance).enableStandingModeOnly();
        PrimaryScript.Instance.GetComponent<SecondaryScript>().setSecondaryInfoVisibility(false);
        PrimaryScript.Instance.GetComponent<SecondaryScript>().setPermanentOverlayTopAccessible(false);
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ScenarioCountdown>().hideCountdown();
        ReferenceAssistor.Instance.module_handlers[2].GetComponent<ScenarioNavigation>().displayEndSequence();
        
        //hide interior for performance
        EndSequenceReferenceAssistor.Instance.deep_space_five_interior.SetActive(false);

        //get references
        end_sequence_camera = EndSequenceReferenceAssistor.Instance.end_sequence_camera;
        end_sequence_ship = EndSequenceReferenceAssistor.Instance.end_sequence_ship;
        end_sequence_arrival_camera_positions = EndSequenceReferenceAssistor.Instance.end_sequence_arrival_camera_positions;
        end_sequence_interaction_camera_positions = EndSequenceReferenceAssistor.Instance.end_sequence_interaction_camera_positions;
        end_sequence_departure_camera_positions = EndSequenceReferenceAssistor.Instance.end_sequence_departure_camera_positions;
        end_sequence_ship_positions = EndSequenceReferenceAssistor.Instance.end_sequence_ship_positions;
        ship_materials = EndSequenceReferenceAssistor.Instance.end_sequence_ship.GetComponent<MeshRenderer>().materials;
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

    //called when the initial cutscene intro is over and the players are in the bridge
    [Rpc(SendTo.Everyone)]
    private void activateBridgeDoorsOpenRPC()
    {
        //unlock player movement
        end_sequence_ship.SetActive(false);
        bridge.transform.localScale = Vector3.one;
        ReferenceAssistor.Instance.lights_manager.default_light_group.SetActive(true);
        hallway.SetActive(true);
        ReferenceAssistor.Instance.player_manager.getLocalPlayer().transform.localScale = Vector3.one;
        unlockPlayerMovement();

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
    private void activateDeepSpaceFiveEntryRPC(string name_one, string name_two, string name_three, string name_four)
    {
        //update the retractable screen signature at the bottom
        GameObject retractable_screen_front_display = EndSequenceReferenceAssistor.Instance.retractable_screen_front_display;
        retractable_screen_front_display.transform.GetChild(3).GetComponent<TMP_Text>().SetText(name_one.ToUpper() + "\n" + name_three.ToUpper());
        retractable_screen_front_display.transform.GetChild(4).GetComponent<TMP_Text>().SetText(name_two.ToUpper() + "\n" + name_four.ToUpper());

        //update the number of shots in the bar
        EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_decorations.transform.GetChild(1).GetChild(1).gameObject.SetActive(string.IsNullOrEmpty(name_two) == false);
        EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_decorations.transform.GetChild(1).GetChild(2).gameObject.SetActive(string.IsNullOrEmpty(name_three) == false);
        EndSequenceReferenceAssistor.Instance.end_sequence_final_cutscene_decorations.transform.GetChild(1).GetChild(3).gameObject.SetActive(string.IsNullOrEmpty(name_four) == false);

        //update the occupancy screen
        int num_players_who_reached_deep_space_five = GameObject.Find("LobbyHandler").GetComponent<LobbyHandler>().getNumberOfPlayersInSteamLobby();
        GameObject occupancy_display = EndSequenceReferenceAssistor.Instance.deep_space_five_occupancy_display;
        string occupancy_percentage = (0.5f * num_players_who_reached_deep_space_five).ToString();
        if (occupancy_percentage.Contains(".") == false)
        {
            occupancy_percentage += ".0";
        }
        occupancy_display.transform.GetChild(2).GetComponent<TMP_Text>().SetText("CAPACITY: " + num_players_who_reached_deep_space_five + "/200 (" + occupancy_percentage + "%)");
        occupancy_display.transform.GetChild(3).GetChild(1).GetComponent<UnityEngine.UI.Image>().fillAmount = num_players_who_reached_deep_space_five * 0.005f;

        StartCoroutine(deepSpaceFiveEntrySequence());
    }

    //called when all players have finished the quick camera transition from elevator to DSF
    [Rpc(SendTo.Everyone)]
    private void activateDeepSpaceFiveFreeRoamRPC()
    {
        //unlock player movement
        unlockPlayerMovement().GetComponent<CameraMove>().UnlockCamera(new Vector2(90.0f, 0.0f));
        ReferenceAssistor.Instance.audio_manager.MuteShipBeeps();
        PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupNotification(1);

        //set interactions
        ReferenceAssistor.Instance.interaction_manager.addInteraction(new InteractionManager.InteractionInfo(DEEP_SPACE_FIVE_INTERACTION_NAMES[0], "INTERACT", ReferenceAssistor.COLOR_OPTIONS[4], EndSequenceReferenceAssistor.Instance.deep_space_five_interaction_points.transform.GetChild(0), INTERACTION_RADIUS, this));
        ReferenceAssistor.Instance.interaction_manager.addInteraction(new InteractionManager.InteractionInfo(DEEP_SPACE_FIVE_INTERACTION_NAMES[1], "INTERACT", ReferenceAssistor.COLOR_OPTIONS[4], EndSequenceReferenceAssistor.Instance.deep_space_five_interaction_points.transform.GetChild(1), INTERACTION_RADIUS, this));

        //open DSF door
        ReferenceAssistor.Instance.translation_helper.linearTranslation(EndSequenceReferenceAssistor.Instance.deep_space_five_gate_b_door.transform, new Vector3(0.0f, 0.0f, -5.6f), 10.0f);
        EndSequenceReferenceAssistor.Instance.deep_space_five_gate_b_door.GetComponent<AudioSource>().Play();

        //start playing music
        EndSequenceReferenceAssistor.Instance.deep_space_five_interior.GetComponent<EndSequenceInteriorFeatures>().playDeepSpaceFiveMusic();
    }

    //returns true if all three interactions are complete (part inventory, computer interface, warp core collection)
    private bool canReturnToShip()
    {
        for (int i = 0; i < 3; i++)
        {
            if (deep_space_five_interactions_complete[i] == false)
            {
                return false;
            }
        }
        return true;
    }

    //called when all players have finished one of the three interaction sequences
    [Rpc(SendTo.Everyone)]
    private void interactionCompleteRPC(bool return_to_ship)
    {
        //unlock player movement
        unlockPlayerMovement();

        //display return to ship notification
        if (return_to_ship == true)
        {
            PrimaryScript.Instance.GetComponent<SecondaryScript>().displayPopupNotification(2);
            EndSequenceReferenceAssistor.Instance.deep_space_five_interior.GetComponent<EndSequenceInteriorFeatures>().enableBoardingSequence();
            if (NetworkManager.Singleton.IsHost == true)
            {
                EndSequenceReferenceAssistor.Instance.end_sequence_board_ship_trigger.GetComponent<EndSequenceBoardShipTriggerHandler>().activate();
            }
        }
    }

    //called when a player uses spacebar to interact with either the part inventory, computer interface, or collectible warp core
    [Rpc(SendTo.Everyone)]
    private void activateDeepSpaceFiveInteractionRPC(int index, ulong interacting_steam_id, string interacting_player_name, int interacting_player_uniform)
    {
        Color dialogue_color = CustomizeCharacterMenu.CLOTHING_OPTION_CORRESPONDING_COLORS[interacting_player_uniform];
        ReferenceAssistor.Instance.interaction_manager.removeInteraction(DEEP_SPACE_FIVE_INTERACTION_NAMES[index]);
        if (deep_space_five_interactions_complete[index] == false)
        {
            deep_space_five_interactions_complete[index] = true;
            if (index == 0) //warp core retrieval
            {
                StartCoroutine(warpCoreRetrievalSequence(interacting_steam_id, interacting_player_name, dialogue_color));

                //enable warp core collection
                ReferenceAssistor.Instance.interaction_manager.addInteraction(new InteractionManager.InteractionInfo(DEEP_SPACE_FIVE_INTERACTION_NAMES[2], "COLLECT", ReferenceAssistor.COLOR_OPTIONS[2], EndSequenceReferenceAssistor.Instance.deep_space_five_interaction_points.transform.GetChild(2), INTERACTION_RADIUS, this));
            }
            else if (index == 1) //computer interface
            {
                StartCoroutine(messageBroadcastSequence(interacting_steam_id, interacting_player_name, dialogue_color));
            }
            else //warp core collection
            {
                EndSequenceReferenceAssistor.Instance.replacement_warp_core.SetActive(false);
                if (SteamClient.SteamId == interacting_steam_id)
                {
                    interactionCompleteRPC(canReturnToShip());
                }
            }
        }
    }

    //called at the very end of the end sequence
    [Rpc(SendTo.Everyone)]
    public void activateFinalCutsceneRPC()
    {
        ignore_lobby_disconnect = true;
        lockPlayerMovement();
        StartCoroutine(finalCutscene());
    }
}