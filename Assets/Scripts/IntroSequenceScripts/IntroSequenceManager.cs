/*
    IntroSequenceManager.cs
    - Used to manage the intro sequence where the player walks around (tutorial)
    Contributor(s): Jake Schott
    Last Updated: 9/28/2026
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntroSequenceManager : MonoBehaviour
{
    //CLASS CONSTANTS
    public static Color[] SEAT_COLORS = new Color[2] { new Color(0.0f, 0.08f, 0.75f), new Color(0.0f, 0.08f, 0.75f) };
    public static string[] SEAT_NAMES = new string[2] { "BRIEFING CONSOLE", "HANGAR CONTROL" };
    public static Vector2[][] SEAT_COORDINATES = new Vector2[2][]
    {
        new Vector2[]{}, //briefing console seat positions
        new Vector2[]{Vector2.zero, new Vector2(-2.5f, 0.0f), new Vector2(-5.5f, 0.0f), new Vector2(-8.0f, 0.0f), new Vector2(-9.75f, 0.0f)} //hangar control seat positions
    };
    public static Vector2[] SEAT_PUSH_IN_ADJUSTMENTS = new Vector2[] 
    { 
        new Vector2(0.7f, 0.0f), //briefing console
        new Vector2(0.0f, -0.7f) //hangar control
    };

    public GameObject player;
    public GameObject cutscene_camera;
    public GameObject opening_scene_camera_positions;
    public GameObject ending_scene_camera_positions;
    public GameObject quarters_room_lights;
    public GameObject quarters_alarm_clock_display;
    public GameObject opening_scene_canvas;
    public GameObject ending_scene_canvas;
    public GameObject hangar_control_lights;
    public GameObject hangar_control_SCC_logos;
    public GameObject hangar_control_navigation_display;
    public GameObject hangar_control_schedule_display;
    public GameObject hangar_control_alerts_display;
    public GameObject hangar_control_access_door_wall_clock_display;
    public GameObject hangar_ship;
    public AudioSource opening_scene_music;
    public AudioSource ambient_noise;
    public AudioSource alarm_sound;
    public AudioSource ending_scene_music;
    public List<GameObject> physical_seats = null;
    public GameObject hangar_door;
    public GameObject hangar_door_display;
    public GameObject hangar_clearance_access_display;

    private int[] seat_indexes = new int[2] { -1, 0 };
    private string hangar_door_clearance_code = "";

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            string digit = Random.Range(1, 10).ToString();
            hangar_door_clearance_code += digit + digit;
        }

        StartCoroutine(openingScene());
    }

    IEnumerator alphaAdjustment(CanvasGroup cg, float starting_a, float ending_a, float time)
    {
        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            cg.alpha = Mathf.Lerp(ending_a, starting_a, anim_time / time);

            yield return null;
        }
    }

    IEnumerator cameraAdjustment(Transform starting_pos, Transform ending_pos, float time)
    {
        cameraPlacement(starting_pos);
        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            cutscene_camera.transform.position = Vector3.Lerp(ending_pos.position, starting_pos.position, anim_time / time);
            cutscene_camera.transform.rotation = Quaternion.Lerp(ending_pos.rotation, starting_pos.rotation, anim_time / time);

            yield return null;
        }
    }

    IEnumerator cameraAdjustmentExponential(Transform starting_pos, Transform ending_pos, float time, bool ease_in)
    {
        cameraPlacement(starting_pos);
        float anim_time = time;
        if (ease_in == false)
        {
            time += 1.0f;
        }
        AnimationCurve animation_curve = AnimationCurve.EaseInOut(0.0f, 0.0f, time, 1.0f);
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float animation_percentage = animation_curve.Evaluate(anim_time);
            cutscene_camera.transform.position = Vector3.Lerp(ending_pos.position, starting_pos.position, animation_percentage);
            cutscene_camera.transform.rotation = Quaternion.Lerp(ending_pos.rotation, starting_pos.rotation, animation_percentage);

            yield return null;
        }
    }

    IEnumerator quartersLightsIllumination(float time)
    {
        float anim_time = time;
        float starting_intensity = 0.1f;
        float ending_intensity = 2.0f;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            float light_intensity = Mathf.Lerp(ending_intensity, starting_intensity, anim_time / time);
            foreach (Transform t in quarters_room_lights.transform)
            {
                t.GetComponent<Light>().intensity = light_intensity;
            }

            yield return null;
        }
    }

    IEnumerator quartersAlarmClockFlash(float time)
    {
        float fill_amount = 1.0f;

        while (time > 0.0f)
        {
            time = Mathf.Max(0.0f, time - Time.deltaTime);

            fill_amount = Mathf.Max(0.0f, fill_amount - (Time.deltaTime * 0.05f));
            quarters_alarm_clock_display.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().fillAmount = fill_amount;

            float a = 0.2f;
            if (Mathf.PingPong(time, 0.2f) < 0.1f)
            {
                a = 1.0f;
            }
            quarters_alarm_clock_display.transform.GetChild(3).GetComponent<TMP_Text>().alpha = a;

            yield return null;
        }

        quarters_alarm_clock_display.transform.GetChild(1).gameObject.SetActive(false);
        quarters_alarm_clock_display.transform.GetChild(2).gameObject.SetActive(true);
        quarters_alarm_clock_display.transform.GetChild(3).GetComponent<TMP_Text>().color = ReferenceAssistor.COLOR_OPTIONS[0];
    }

    private void cameraPlacement(Transform pos)
    {
        cutscene_camera.transform.position = pos.position;
        cutscene_camera.transform.rotation = pos.rotation;
    }

    IEnumerator openingScene()
    {
        opening_scene_canvas.SetActive(true);
        yield return new WaitForSeconds(1.0f);

        Cursor.lockState = CursorLockMode.Locked;
        GameObject.Find("LoadHandler").GetComponent<LoadHandler>().endLoad(false);

        yield return new WaitForSeconds(2.0f);

        StartCoroutine(cameraAdjustment(opening_scene_camera_positions.transform.GetChild(0), opening_scene_camera_positions.transform.GetChild(1), 16.0f));
        StartCoroutine(alphaAdjustment(opening_scene_canvas.transform.GetChild(0).GetChild(0).GetComponent<CanvasGroup>(), 1.0f, 0.8f, 6.0f));

        opening_scene_music.Play();
        ambient_noise.Play();

        yield return new WaitForSeconds(2.5f);

        yield return alphaAdjustment(opening_scene_canvas.transform.GetChild(0).GetChild(1).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f);

        yield return new WaitForSeconds(1.0f);

        yield return alphaAdjustment(opening_scene_canvas.transform.GetChild(0).GetChild(2).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f);

        yield return new WaitForSeconds(2.5f);

        yield return alphaAdjustment(opening_scene_canvas.transform.GetChild(0).GetChild(3).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f);

        yield return new WaitForSeconds(3.5f);

        StartCoroutine(quartersLightsIllumination(20.0f));
        StartCoroutine(quartersAlarmClockFlash(15.0f));
        StartCoroutine(alphaAdjustment(opening_scene_canvas.transform.GetChild(0).GetComponent<CanvasGroup>(), 1.0f, 0.0f, 5.0f));
        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(2), opening_scene_camera_positions.transform.GetChild(3), 6.5f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(4), opening_scene_camera_positions.transform.GetChild(5), 7.5f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(6), opening_scene_camera_positions.transform.GetChild(7), 9.0f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(8), opening_scene_camera_positions.transform.GetChild(9), 4.0f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(10), opening_scene_camera_positions.transform.GetChild(11), 4.5f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(12), opening_scene_camera_positions.transform.GetChild(13), 4.5f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(14), opening_scene_camera_positions.transform.GetChild(15), 3.0f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(16), opening_scene_camera_positions.transform.GetChild(17), 3.5f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(18), opening_scene_camera_positions.transform.GetChild(19), 3.0f);

        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(20), opening_scene_camera_positions.transform.GetChild(21), 3.5f);

        player.SetActive(true);
        yield return cameraAdjustment(opening_scene_camera_positions.transform.GetChild(22), opening_scene_camera_positions.transform.GetChild(23), 3.5f);

        cutscene_camera.gameObject.SetActive(false);
        player.GetComponent<CameraMove>().GetCamera().SetActive(true);
        PrimaryScript.Instance.unlockPlayer(player);
    }

    public void endTutorial()
    {
        StartCoroutine(endingScene());
    }

    IEnumerator hangarLightsRedFlash()
    {
        float r = 1.0f;
        float elapsed_time = 0.0f;
        while (true)
        {
            elapsed_time += Time.deltaTime;

            r = (Mathf.PingPong(elapsed_time, 0.5f) / 0.5f);
            Color c = new Color(r, 0.0f, 0.0f);
            foreach (Transform g in hangar_control_lights.transform)
            {
                foreach (Transform l in g)
                {
                    l.GetComponent<Light>().color = c;
                }
            }
            foreach (Transform t in hangar_control_SCC_logos.transform)
            {
                t.GetChild(1).GetComponent<SpriteRenderer>().color = c;
            }

            yield return null;
        }
    }

    IEnumerator navigationShipMovement(float time)
    {
        GameObject ship = hangar_control_navigation_display.transform.GetChild(6).gameObject;
        ship.SetActive(true);

        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            ship.transform.localPosition = Vector3.Lerp(new Vector3(1.54f, 0.265f, 0.0f), new Vector3(0.5f, -0.775f, 0.0f), anim_time / time);

            yield return null;
        }
    }

    IEnumerator soundVolumeAdjuster(AudioSource source, float ending_volume, float time)
    {
        float starting_volume = source.volume;

        float anim_time = time;
        while (anim_time > 0.0f)
        {
            anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);

            source.volume = Mathf.Lerp(ending_volume, starting_volume, anim_time / time);

            yield return null;
        }
    }

    private void enableRedHangarControlScreens()
    {
        hangar_control_navigation_display.transform.GetChild(1).GetComponent<TMP_Text>().color = Color.red;
        hangar_control_navigation_display.transform.GetChild(2).GetComponent<UnityEngine.UI.RawImage>().color = new Color(1.0f, 0.0f, 0.0f, 0.08f);
        hangar_control_navigation_display.transform.GetChild(3).GetChild(0).GetComponent<UnityEngine.UI.RawImage>().color = Color.red;
        hangar_control_navigation_display.transform.GetChild(5).GetComponent<UnityEngine.UI.RawImage>().color = Color.red;
        hangar_control_schedule_display.transform.GetChild(1).gameObject.SetActive(false);
        hangar_control_schedule_display.transform.GetChild(2).gameObject.SetActive(true);
        hangar_control_alerts_display.transform.GetChild(1).gameObject.SetActive(false);
        hangar_control_alerts_display.transform.GetChild(2).gameObject.SetActive(true);
        hangar_control_access_door_wall_clock_display.transform.GetChild(1).GetComponent<TMP_Text>().color = Color.red;
    }

    IEnumerator endingScene()
    {
        cutscene_camera.gameObject.SetActive(true);
        PrimaryScript.Instance.deactivate(false, false);
        player.GetComponent<CameraMove>().GetCamera().SetActive(false);
        player.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        yield return cameraAdjustmentExponential(ending_scene_camera_positions.transform.GetChild(0), ending_scene_camera_positions.transform.GetChild(1), 7.0f, false);

        StartCoroutine(cameraAdjustmentExponential(ending_scene_camera_positions.transform.GetChild(1), ending_scene_camera_positions.transform.GetChild(2), 4.0f, true));
        yield return new WaitForSeconds(2.0f);

        alarm_sound.Play();
        StartCoroutine(hangarLightsRedFlash());
        yield return new WaitForSeconds(2.0f);

        ending_scene_music.Play();
        enableRedHangarControlScreens();
        hangar_ship.SetActive(false);
        StartCoroutine(navigationShipMovement(14.0f));
        yield return cameraAdjustmentExponential(ending_scene_camera_positions.transform.GetChild(3), ending_scene_camera_positions.transform.GetChild(4), 20.0f, false);

        yield return cameraAdjustmentExponential(ending_scene_camera_positions.transform.GetChild(4), ending_scene_camera_positions.transform.GetChild(5), 10.0f, true);

        ReferenceAssistor.Instance.module_handlers[1].GetComponent<ControlConsoleSpaceDoors>().closeDoors(12.0f);
        StartCoroutine(soundVolumeAdjuster(alarm_sound, 0.0f, 15.0f));
        StartCoroutine(cameraAdjustment(ending_scene_camera_positions.transform.GetChild(6), ending_scene_camera_positions.transform.GetChild(7), 10.0f));
        yield return new WaitForSeconds(1.0f);

        ending_scene_canvas.SetActive(true);
        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(1).GetChild(0).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f));

        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(1).GetChild(1).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f));

        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(1).GetChild(2).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f));

        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(1).GetChild(3).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.0f));

        yield return new WaitForSeconds(4.0f);

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(0).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 1.5f));

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(1).GetComponent<CanvasGroup>(), 1.0f, 0.0f, 2.0f));

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(2).GetComponent<CanvasGroup>(), 0.0f, 1.0f, 2.0f));

        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(alphaAdjustment(ending_scene_canvas.transform.GetChild(2).GetComponent<CanvasGroup>(), 1.0f, 0.0f, 2.0f));

        yield return new WaitForSeconds(1.0f);

        UnityEngine.Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        PlayerManager.leaveGame();
    }

    public void checkForDoorUnlock()
    {
        bool door_unlocked = (ReferenceAssistor.Instance.module_handlers[1].GetComponent<ControlConsoleSpaceDoors>().doorIsOpen() && ReferenceAssistor.Instance.module_handlers[1].GetComponent<ControlConsoleClearanceCode>().codeIsCorrect());

        hangar_door_display.transform.GetChild(1).gameObject.SetActive(!door_unlocked);
        hangar_door_display.transform.GetChild(2).gameObject.SetActive(door_unlocked);

        hangar_door.transform.GetChild(2).gameObject.SetActive(door_unlocked);

        if (door_unlocked == true)
        {
            hangar_door.transform.GetChild(3).gameObject.GetComponent<Renderer>().material = ReferenceAssistor.Instance.pure_black;
            hangar_door.transform.GetChild(4).gameObject.GetComponent<Renderer>().material = ReferenceAssistor.Instance.lit_green;
        }
        else
        {
            hangar_door.transform.GetChild(3).gameObject.GetComponent<Renderer>().material = ReferenceAssistor.Instance.lit_red;
            hangar_door.transform.GetChild(4).gameObject.GetComponent<Renderer>().material = ReferenceAssistor.Instance.pure_black;
        }

        hangar_clearance_access_display.transform.GetChild(1).gameObject.SetActive(!door_unlocked);
        hangar_clearance_access_display.transform.GetChild(2).gameObject.SetActive(door_unlocked);
    }

    public int checkSeats(Vector3 player_pos)
    {
        for (int i = 0; i < physical_seats.Count; i++)
        {
            for (int x = 0; x < 2; x++)
            {
                if (Vector3.Distance(player_pos, physical_seats[i].transform.GetChild(x).transform.position) < SeatManager.SIT_RANGE)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public bool getSitDownDirection(int seat, Vector3 player_pos)
    {
        float left_check = Vector3.Distance(player_pos, physical_seats[seat].transform.GetChild(0).transform.position);
        float right_check = Vector3.Distance(player_pos, physical_seats[seat].transform.GetChild(1).transform.position);
        if (left_check > right_check)
        {
            return true;
        }
        return false;
    }

    public GameObject getSitDownPosition(int seat, Vector3 player_pos)
    {
        if (getSitDownDirection(seat, player_pos) == true)
        {
            return physical_seats[seat].transform.GetChild(1).gameObject;
        }
        return physical_seats[seat].transform.GetChild(0).gameObject;
    }

    //returns true if able to shift left
    public bool canShiftLeft(int seat)
    {
        if (SEAT_COORDINATES[seat].Length == 0)
        {
            return false;
        }
        return (seat_indexes[seat] > 0);
    }

    //returns true if able to shift right
    public bool canShiftRight(int seat)
    {
        if (SEAT_COORDINATES[seat].Length == 0)
        {
            return false;
        }
        return (seat_indexes[seat] < (SEAT_COORDINATES[seat].Length - 1));
    }

    public int getShiftLocation(int seat, bool look_direction)
    {
        if (seat_indexes[seat] == 0) //if furthest left, one to the right
        {
            return 1;
        }
        if (seat_indexes[seat] == SEAT_COORDINATES[seat].Length - 1) //if furthest right, one to the left
        {
            return SEAT_COORDINATES[seat].Length - 2;
        }
        if (look_direction == false) //if looking right
        {
            return seat_indexes[seat] + 1; //right
        }
        return seat_indexes[seat] - 1; //left
    }

    public void updateSeatIndex(int seat, int new_index)
    {
        seat_indexes[seat] = new_index;
    }

    public bool getGetUpDirection(int seat)
    {
        return true;
    }

    public string getClearanceCode()
    {
        return hangar_door_clearance_code;
    }
}