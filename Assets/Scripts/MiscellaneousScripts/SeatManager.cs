/*
    SeatManager.cs
    - Used to ensure two players are not sitting in the same seat at the same time
    - Checks if a player is close enough to sit down
    Contributor(s): Jake Schott
    Last Updated: 6/7/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SeatManager : NetworkBehaviour
{
    //CLASS CONSTANTS
    private static float SIT_RANGE = 0.5f;

    //GAME OBJECTS
    public List<GameObject> position_points = null; //empties that are used to check if a player is close enough to sit
    public List<GameObject> left_shift_position_points = null; //shift left positions
    public List<GameObject> right_shift_position_points = null; //shift right positions

    private bool[] occupied_seats = new bool[4] { false, false, false, false };

    public int checkSeats(Vector3 player_pos)
    {
        float closest_dist = 9999.9f;
        int closest_pos = -1;
        for (int i = 0; i < position_points.Count; i++)
        {
            float test_dist = Vector3.Distance(player_pos, position_points[i].transform.position);
            if (test_dist < closest_dist)
            {
                closest_dist = test_dist;
                if (closest_dist < SIT_RANGE && occupied_seats[i] == false)
                {
                    closest_pos = i;
                }
            }
        }
        return closest_pos;
    }

    public bool sitDown(Vector3 player_pos)
    {
        int seat = checkSeats(player_pos);
        if (seat >= 0)
        {
            transmitSeatChangeRPC(seat, true);
            return true;
        }
        return false;
    }

    public bool getUp(int seat)
    {
        if (occupied_seats[seat] == true)
        {
            transmitSeatChangeRPC(seat, false);
            return true;
        }
        return false;
    }

    [Rpc(SendTo.Everyone)]
    private void transmitSeatChangeRPC(int seat, bool occupied)
    {
        occupied_seats[seat] = occupied;
    }
}
