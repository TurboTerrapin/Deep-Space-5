/*
    EndSequenceReferenceAssistor.cs
    - Used to reference objects in the end sequence scene
    Contributor(s): Jake Schott
    Last Updated: 11/24/2026
*/

using UnityEngine;

public class EndSequenceReferenceAssistor : MonoBehaviour
{
    public GameObject end_sequence_camera;
    public GameObject end_sequence_arrival_camera_positions;
    public GameObject end_sequence_interaction_camera_positions;
    public GameObject end_sequence_departure_camera_positions;
    public GameObject end_sequence_ship;
    public GameObject end_sequence_ship_positions;
    public GameObject end_sequence_cutscene_sounds;
    public GameObject deep_space_five_interior;
    public GameObject deep_space_five_spawn_points;
    public GameObject deep_space_five_players_holder;
    public GameObject deep_space_five_occupancy_display;
    public GameObject deep_space_five_gate_b_door;
    public GameObject deep_space_five_speaker_music;
    public GameObject deep_space_five_interaction_points;
    public GameObject deep_space_five_animation_points;
    public GameObject part_inventory_selector_display;
    public GameObject part_inventory_progress_display;
    public GameObject part_inventory_key_boop_sound;
    public GameObject part_inventory_progress_sound;
    public GameObject part_inventory_doors;
    public GameObject replacement_warp_core;
    public GameObject computer_interface_display;
    public GameObject computer_interface_boop_sound;
    public GameObject retractable_screen;
    public GameObject retractable_screen_front_display;
    public GameObject end_sequence_board_ship_trigger;
    public GameObject end_sequence_final_cutscene_decorations;
    public GameObject[] end_sequence_final_cutscene_lights = new GameObject[7];
    public GameObject[] end_sequence_ship_engine_lights = new GameObject[5];
    public GameObject warp_portal;

    public static EndSequenceReferenceAssistor Instance { get; private set; }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
}
