/*
    CrewManifest.cs
    - Handles displaying crew member names from within the bridge
    Contributor(s): Jake Schott
    Last Updated: 11/25/2026
*/

using System.Collections.Generic;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CrewManifest : NetworkBehaviour
{
    public GameObject crew_manifest_display;

    private Dictionary<ulong, string> crew_member_names = new Dictionary<ulong, string>(); //key steam ID, value crew member name (ex. "JAMES KIRK")

    public void reportAsReady()
    {
        string crew_member_name = CustomizeCharacterMenu.GetFirstName().ToUpper() + " " + CustomizeCharacterMenu.GetLastName().ToUpper();
        transmitCrewMemberNameRPC(SteamClient.SteamId, crew_member_name);
    }

    public string getPlayerFirstInitialAndLastName(ulong player_id)
    {
        if (crew_member_names.ContainsKey(player_id) == true)
        {
            string character_name = crew_member_names[player_id];
            string abbreviated_name = character_name[0] + ". ";
            abbreviated_name += character_name.Substring(character_name.IndexOf(" ") + 1);
            return abbreviated_name;
        }
        return "";
    }

    public void updateCrewManifest()
    {
        GameObject lobby_handler = GameObject.Find("LobbyHandler");
        if (lobby_handler == null)
        {
            return;
        }

        List<ulong> plr_steam_ids = lobby_handler.GetComponent<LobbyHandler>().getPlayerSteamIDsInLobby();
        for (int i = 0; i < 4; i++)
        {
            if (i < plr_steam_ids.Count && crew_member_names.ContainsKey(plr_steam_ids[i]) == true)
            {
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().SetText("• " + getPlayerFirstInitialAndLastName(plr_steam_ids[i]));
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().alpha = 1.0f;
            }
            else
            {
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().SetText("• ------------------------");
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().alpha = 0.2f;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void transmitCrewMemberNameRPC(ulong steam_id, string crew_member_name)
    {
        if (crew_member_names.ContainsKey(steam_id) == false)
        {
            crew_member_names.Add(steam_id, crew_member_name);
        }
        updateCrewManifest();
    }
}