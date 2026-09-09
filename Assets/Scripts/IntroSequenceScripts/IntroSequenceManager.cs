/*
    IntroSequenceManager.cs
    - Used to manage the intro sequence where the player walks around (tutorial)
    Contributor(s): Jake Schott
    Last Updated: 9/8/2026
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntroSequenceManager : MonoBehaviour
{
    //CLASS CONSTANTS
    public static Color[] SEAT_COLORS = new Color[1] { new Color(0.0f, 0.08f, 0.75f) };
    public static string[] SEAT_NAMES = new string[1] { "BRIEFING CONSOLE" };
    public static Vector2[][] SEAT_COORDINATES = new Vector2[1][]
    {
        new Vector2[]{}, //briefing console seat positions
    };
    public static Vector2[] SEAT_PUSH_IN_ADJUSTMENTS = new Vector2[] 
    { 
        new Vector2(0.7f, 0.0f) //briefing console
    };
    public static string[] SHAPE_NAMES = new string[4] { "CIRCLE", "SQUARE", "TRIANGLE", "HEXAGON" };

    public GameObject player;
    public GameObject cutscene_camera;
    public List<GameObject> physical_seats = null;

    private string hangar_door_override_code = "";
    private int hangar_door_override_shape = 0; //0 circle, 1 square, 2 triangle, 3 hexagon

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            hangar_door_override_code += Random.Range(0, 10).ToString();
        }
        hangar_door_override_shape = Random.Range(0, 4);

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

    public bool getGetUpDirection(int seat)
    {
        return true;
    }

    public int getOverrideShape()
    {
        return hangar_door_override_shape;
    }

    public string getOverrideCode()
    {
        return hangar_door_override_code;
    }
}