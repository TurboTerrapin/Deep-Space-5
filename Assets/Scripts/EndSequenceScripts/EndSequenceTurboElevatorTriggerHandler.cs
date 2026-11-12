/*
    EndSequenceTurboElevatorTriggerHandler.cs
    - Used to close the elevator when all players are in it during the end sequence
    Contributor(s): Jake Schott
    Last Updated: 10/27/2026
*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class EndSequenceTurboElevatorTriggerHandler : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float ELEVATOR_CHECK_TIME = 1.0f;

    private List<GameObject> players_in_trigger = new List<GameObject>();
    private LobbyHandler lobby_handler;
    private Coroutine elevator_check_coroutine = null;

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost == false)
        {
            GameObject.Destroy(gameObject);
        }
        lobby_handler = GameObject.Find("LobbyHandler").GetComponent<LobbyHandler>();
    }

    private bool allPlayersInElevator()
    {
        return (players_in_trigger.Count == lobby_handler.getNumberOfPlayersInNetworkManagerLobby());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.CompareTo("Player") == 0 && players_in_trigger.Contains(other.gameObject) == false)
        {
            players_in_trigger.Add(other.gameObject);
        }
        if (elevator_check_coroutine == null && allPlayersInElevator() == true)
        {
            elevator_check_coroutine = StartCoroutine(elevatorCheck());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (players_in_trigger.Contains (other.gameObject) == true)
        {
            players_in_trigger.Remove(other.gameObject);
        }
    }

    IEnumerator elevatorCheck()
    {
        float check_time = ELEVATOR_CHECK_TIME;
        while (check_time > 0.0f && allPlayersInElevator() == true)
        {
            check_time = check_time - Time.deltaTime;

            yield return null;
        }

        if (allPlayersInElevator() == true)
        {
            ReferenceAssistor.Instance.end_sequence_manager.activateTurboElevatorRPC();
            GameObject.Destroy(gameObject);
        }
        elevator_check_coroutine = null;
    }
}
