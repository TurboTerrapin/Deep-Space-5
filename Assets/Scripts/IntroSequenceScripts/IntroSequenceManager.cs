/*
    IntroSequenceManager.cs
    - Used to manage the intro sequence where the player walks around (tutorial)
    Contributor(s): Jake Schott
    Last Updated: 9/18/2026
*/

using System.Collections;
using System.Collections.Generic;
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
    public List<GameObject> physical_seats = null;
    public GameObject hangar_door;
    public GameObject hangar_door_display;

    private int[] seat_indexes = new int[2] { -1, 0 };
    private string hangar_door_clearance_code = "";

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            string digit = Random.Range(1, 10).ToString();
            hangar_door_clearance_code += digit + digit;
        }

        StartCoroutine(yieldForLoad());
    }

    IEnumerator yieldForLoad()
    {
        yield return new WaitForSeconds(1.0f);

        cutscene_camera.SetActive(false);
        GameObject.Find("LoadHandler").GetComponent<LoadHandler>().endLoad(true);
        player.GetComponent<CameraMove>().GetCamera().SetActive(true);
        PrimaryScript.Instance.unlockPlayer(player);
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