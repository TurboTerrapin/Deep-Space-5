/*
    EndSequenceBoardShipTriggerHandler.cs
    - Used to trigger the final cutscene
    Contributor(s): Jake Schott
    Last Updated: 11/21/2026
*/

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndSequenceBoardShipTriggerHandler : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float GATE_CHECK_TIME = 1.0f;

    private List<GameObject> players_in_trigger = new List<GameObject>();
    private LobbyHandler lobby_handler;
    private Coroutine gate_check_coroutine = null;
    private bool active = false;

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost == false)
        {
            GameObject.Destroy(gameObject);
        }
        lobby_handler = GameObject.Find("LobbyHandler").GetComponent<LobbyHandler>();
    }

    private bool allPlayersAtGate()
    {
        int confirmed_players = 0;
        for (int i = 0; i < players_in_trigger.Count; i++)
        {
            if (players_in_trigger[i] != null)
            {
                confirmed_players++;
            }
        }
        return (confirmed_players == lobby_handler.getNumberOfPlayersInSteamLobby());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.CompareTo("Player") == 0 && players_in_trigger.Contains(other.gameObject) == false)
        {
            players_in_trigger.Add(other.gameObject);
        }
        if (gate_check_coroutine == null && allPlayersAtGate() == true && active == true)
        {
            gate_check_coroutine = StartCoroutine(gateCheck());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (players_in_trigger.Contains(other.gameObject) == true)
        {
            players_in_trigger.Remove(other.gameObject);
        }
    }

    public void activate()
    {
        active = true;
        gate_check_coroutine = StartCoroutine(gateCheck());
    }

    IEnumerator gateCheck()
    {
        float check_time = GATE_CHECK_TIME;
        while (check_time > 0.0f && allPlayersAtGate() == true)
        {
            check_time = check_time - Time.deltaTime;

            yield return null;
        }

        if (allPlayersAtGate() == true)
        {
            ReferenceAssistor.Instance.end_sequence_manager.activateFinalCutsceneRPC();
            GameObject.Destroy(gameObject);
        }
        gate_check_coroutine = null;
    }
}