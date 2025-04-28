/*
    UniversalCommunicator.cs
    - Handles inputs for communicator keyboard
    - Displays to code screen
    Contributor(s): Jake Schott
    Last Updated: 4/28/2025
*/

using UnityEngine;
using System.Collections.Generic;
public class UniversalCommunicator : MonoBehaviour, IControllable
{
    private string[] CONTROL_NAMES = new string[] {"CHARACTER INPUT"};
    private List<string> CONTROL_DESCS = new List<string> { "INPUT" };
    private List<int> CONTROL_INDEXES = new List<int>() {6};
    private List<Button>[] BUTTON_LISTS = new List<Button>[1];

    public List<GameObject> character_displays = null;
    public GameObject code_display;
    public List<GameObject> character_buttons = null;

    private bool selection_cooling_down = false;
    private int cursor_index = 0;

    private List<KeyCode> keys_down = new List<KeyCode>();

    private List<string> ray_targets = new List<string> {"A0", "A1", "A2", "A3", "A4", "A5", "B0", "B1", "B2", "B3", "B4", "B5"};
    private int target_index = 0;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAMES[0]);

        BUTTON_LISTS[0] = new List<Button>();
        BUTTON_LISTS[0].Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));

    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        int index = ray_targets.IndexOf(current_target.name);
        if (index < 12)
        {
            hud_info.setTitle(CONTROL_NAMES[0]);
            hud_info.setButtons(BUTTON_LISTS[0]);
        }

        return hud_info;
    }
    private void displayAdjustment(int index)
    {
 
    }
    void Update()
    {
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        target_index = ray_targets.IndexOf(current_target.name);
        keys_down = inputs;
    }
}
