/*
    CrewManifest.cs
    - Handles displaying crew member names from within the bridge
    Contributor(s): Jake Schott
    Last Updated: 9/8/2026
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
        string crew_member_name = CustomizeCharacterMenu.GetFirstName() + " " + CustomizeCharacterMenu.GetLastName();
        transmitCrewMemberNameRPC(SteamClient.SteamId, crew_member_name);
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
                string character_name = crew_member_names[plr_steam_ids[i]];
                string display_name = character_name[0] + ". ";
                display_name += character_name.Substring(character_name.IndexOf(" ") + 1);
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().SetText("• " + display_name);
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().color = new Color(0.0f, 0.84f, 1.0f, 1.0f);
            }
            else
            {
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().SetText("• ------------------------");
                crew_manifest_display.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>().color = new Color(0.0f, 0.84f, 1.0f, 0.2f);
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