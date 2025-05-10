/*
    Map.cs
    - Handles inputs for map zoom, map configuration
    - Updates map
    Contributor(s): Jake Schott
    Last Updated: 5/8/2025
*/

using UnityEngine;
using System.Collections.Generic;

public class Map : MonoBehaviour, IControllable
{
    private string CONTROL_NAME = "MAP OPTIONS";
    private List<string> CONTROL_DESCS = new List<string> {"CHANGE MODE", "ZOOM OUT", "ZOOM IN"};
    private List<int> CONTROL_INDEXES = new List<int>() {6, 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject slider;
    public GameObject configuration_button;
    public GameObject configuration_canvas;
    public GameObject map_canvas;
    private Vector3 initial_config_button_pos;
    private Vector3 config_push_direction = new Vector3(0f, -0.0061f, -0.0023f);

    private List<KeyCode> keys_down = new List<KeyCode>();

    private float zoom = 1.0f;
    private Vector3 initial_pos; //slider starting position (100% zoom)
    private Vector3 final_pos = new Vector3(0f, -0.0375f, 0.0888f); //slider final position (0% impulse)

    private int map_config = 0;
    private bool map_config_cooling_down = false;
    private float map_config_cooldown_timer = 0.0f;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], false, false));
        hud_info.setButtons(BUTTONS, 5);
        hud_info.adjustButtonFontSizes(36.0f);

        initial_pos = slider.transform.position; //sets the initial position
        initial_config_button_pos = configuration_button.transform.position;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }
    private void displayAdjustment()
    {
        for (int i = 0; i < 6; i++)
        {
            float circle_diameter = 0.06f + (0.04f * (5f - i)) + (zoom * (0.06f + (0.04f * (5f - i))));
            map_canvas.transform.GetChild(1).GetChild(i).gameObject.SetActive(!(circle_diameter > 0.31f));

            map_canvas.transform.GetChild(1).GetChild(i).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(circle_diameter, circle_diameter);
            map_canvas.transform.GetChild(1).GetChild(i).GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(circle_diameter - 0.005f, circle_diameter - 0.005f);
        }
        if (map_config_cooling_down == true)
        {
            float push_distance = 1f - ((map_config_cooldown_timer - 0.25f) / 0.25f);
            if (map_config_cooldown_timer <= 0.25f)
            {
                push_distance = (map_config_cooldown_timer / 0.25f);
            }
            configuration_button.transform.localPosition =
                new Vector3(initial_config_button_pos.x,
                            initial_config_button_pos.y + push_distance * config_push_direction.y,
                            initial_config_button_pos.z + push_distance * config_push_direction.z);
        }
        //update lever position
        slider.transform.position = new Vector3(0, initial_pos.y + final_pos.y * (1f - zoom), initial_pos.z + final_pos.z * (1f - zoom));
    }
    void Update()
    {
        int zoom_direction = 0;
        if (keys_down.Contains(KeyCode.E) || keys_down.Contains(KeyCode.RightArrow)) //E to zoom in
        {
            zoom_direction += 1;
        }
        if (keys_down.Contains(KeyCode.Q) || keys_down.Contains(KeyCode.LeftArrow))  //Q to zoom out
        {
            zoom_direction -= 1;
        }
        if (zoom_direction != 0)
        {
            if (zoom_direction > 0)
            {
                zoom = Mathf.Min(1.0f, zoom + Time.deltaTime);
            }
            else
            {
                zoom = Mathf.Max(0.0f, zoom - Time.deltaTime);
            }
            if (zoom <= 0f)
            {
                hud_info.getButtons()[1].updateInteractable(false);
            }
            else
            {
                hud_info.getButtons()[1].updateInteractable(true);
            }
            if (zoom >= 1f)
            {
                hud_info.getButtons()[2].updateInteractable(false);
            }
            else
            {
                hud_info.getButtons()[2].updateInteractable(true);
            }
        }
        if (map_config_cooling_down == false)
        {
            if (keys_down.Contains(KeyCode.Mouse0) || keys_down.Contains(KeyCode.KeypadEnter))
            {
                BUTTONS[0].toggle(0.2f);
                BUTTONS[0].updateInteractable(false);
                map_config += 1;
                map_config_cooldown_timer = 0.5f;
                map_config_cooling_down = true;
            }
        }
        else
        {
            map_config_cooldown_timer = Mathf.Max(0.0f, map_config_cooldown_timer - Time.deltaTime);
            if (map_config_cooldown_timer <= 0.0f)
            {
                BUTTONS[0].updateInteractable(true);
                map_config_cooling_down = false;
            }
        }
        displayAdjustment();
        keys_down.Clear();
    }
    public void handleInputs(List<KeyCode> inputs, GameObject current_target, int position)
    {
        keys_down = inputs;
    }
}
