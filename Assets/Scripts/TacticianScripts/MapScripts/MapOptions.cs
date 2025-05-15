/*
    MapOptions.cs
    - Handles inputs for map zoom, map configuration
    - Updates map
    Contributor(s): Jake Schott
    Last Updated: 5/14/2025
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapOptions : NetworkBehaviour, IControllable
{
    //CLASS CONSTANTS
    private static float ZOOM_SPEED = 1.0f;
    private static float PUSH_TIME = 0.5f;

    private string CONTROL_NAME = "MAP OPTIONS";
    private List<string> CONTROL_DESCS = new List<string> {"CHANGE MODE", "ZOOM OUT", "ZOOM IN"};
    private List<int> CONTROL_INDEXES = new List<int>() {6, 4, 5 };
    private List<Button> BUTTONS = new List<Button>();

    public GameObject slider;
    public GameObject config_button;
    public GameObject config_canvas;
    public GameObject map_canvas;

    private Vector3 config_button_initial_pos;
    private Vector3 config_button_final_pos = new Vector3(0, -0.01756f, -17.8906f);

    //zoom settings
    private float zoom = 1.0f;
    private Vector3 slider_initial_pos; //slider starting position (100% zoom)
    private Vector3 slider_final_pos = new Vector3(0, -0.04983f, -17.8025f);

    private int map_config = 0;
    private Coroutine map_config_coroutine = null;

    private static HUDInfo hud_info = null;
    private void Start()
    {
        hud_info = new HUDInfo(CONTROL_NAME);
        BUTTONS.Add(new Button(CONTROL_DESCS[0], CONTROL_INDEXES[0], true, true));
        BUTTONS.Add(new Button(CONTROL_DESCS[1], CONTROL_INDEXES[1], true, false));
        BUTTONS.Add(new Button(CONTROL_DESCS[2], CONTROL_INDEXES[2], false, false));
        hud_info.setButtons(BUTTONS, 5);
        hud_info.adjustButtonFontSizes(36.0f);

        //set initial positions
        slider_initial_pos = slider.transform.localPosition;
        config_button_initial_pos = config_button.transform.localPosition;
    }
    public HUDInfo getHUDinfo(GameObject current_target)
    {
        return hud_info;
    }

    IEnumerator adjustMapConfig()
    {
        for (int i = 0; i <= 1; i++)
        {
            float half_time = PUSH_TIME * 0.5f;
            float push_time = half_time;

            while (push_time > 0.0f)
            {
                float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
                push_time = Mathf.Max(0.0f, push_time - dt);

                float push_percentage = 1.0f - (push_time / half_time);
                if (i == 1)
                {
                    push_percentage = (push_time / half_time);
                }

                config_button.transform.localPosition =
                    new Vector3(Mathf.Lerp(config_button_initial_pos.x, config_button_final_pos.x, push_percentage),
                                Mathf.Lerp(config_button_initial_pos.y, config_button_final_pos.y, push_percentage),
                                Mathf.Lerp(config_button_initial_pos.z, config_button_final_pos.z, push_percentage));

                yield return null;
            }
        }

        BUTTONS[0].updateInteractable(true);

        map_config_coroutine = null;
    }

    private void displayZoomAdjustment()
    {
        //zoom map
        for (int i = 0; i < 6; i++)
        {
            float circle_diameter = 0.06f + (0.04f * (5f - i)) + (zoom * (0.06f + (0.04f * (5f - i))));
            map_canvas.transform.GetChild(1).GetChild(i).gameObject.SetActive(!(circle_diameter > 0.31f));

            map_canvas.transform.GetChild(1).GetChild(i).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(circle_diameter, circle_diameter);
            map_canvas.transform.GetChild(1).GetChild(i).GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(circle_diameter - 0.005f, circle_diameter - 0.005f);
        }

        //update zoom slider position
        slider.transform.position =
            new Vector3(Mathf.Lerp(slider_initial_pos.x, slider_final_pos.x, 1.0f - zoom),
                        Mathf.Lerp(slider_initial_pos.y, slider_final_pos.y, 1.0f - zoom),
                        Mathf.Lerp(slider_initial_pos.z, slider_final_pos.z, 1.0f - zoom));
    }

    public void handleInputs(List<KeyCode> inputs, GameObject current_target, float dt, int position)
    {
        //check zoom inputs
        int zoom_direction = 0;
        if (ControlScript.checkInputIndex(CONTROL_INDEXES[2], inputs)) //E to increment
        {
            zoom_direction += 1;
        }
        if (ControlScript.checkInputIndex(CONTROL_INDEXES[1], inputs))  //Q to decrement
        {
            zoom_direction -= 1;
        }
        if (zoom_direction != 0)
        {
            if (zoom_direction > 0)
            {
                zoom = Mathf.Min(1.0f, zoom + dt * ZOOM_SPEED);
            }
            else
            {
                zoom = Mathf.Max(0.0f, zoom - dt * ZOOM_SPEED);
            }
            if (zoom <= 0f)
            {
                BUTTONS[1].updateInteractable(false);
            }
            else
            {
                BUTTONS[1].updateInteractable(true);
            }
            if (zoom >= 1f)
            {
                BUTTONS[2].updateInteractable(false);
            }
            else
            {
                BUTTONS[2].updateInteractable(true);
            }
            transmitMapZoomAdjustmentRPC(zoom);
        }

        //check map config button
        if (map_config_coroutine == null)
        {
            if (ControlScript.checkInputIndex(CONTROL_INDEXES[0], inputs))
            {
                BUTTONS[0].toggle(0.2f);
                transmitMapConfigurationAdjustmentRPC(map_config);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitMapZoomAdjustmentRPC(float zm)
    {
        zoom = zm;
        displayZoomAdjustment();
    }

    [Rpc(SendTo.Everyone)]
    private void transmitMapConfigurationAdjustmentRPC(int mc)
    {
        map_config = mc;
        if (map_config_coroutine != null)
        {
            StopCoroutine(map_config_coroutine);
        }
        map_config_coroutine = StartCoroutine(adjustMapConfig());
    }
}
