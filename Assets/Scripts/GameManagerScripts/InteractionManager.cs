/*
    InteractionManager.cs
    - Helps with moving things around in a linear or exponential fashion
    Contributor(s): Jake Schott
    Last Updated: 11/24/2026
*/

using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public struct InteractionInfo
    {
        public string interaction_name;
        public string interaction_action_label;
        public Color interaction_color;
        public Transform interaction_point;
        public float interaction_radius;
        public IInteractable interaction_handler;

        public InteractionInfo(string name, string action, Color color, Transform point, float radius, IInteractable handler)
        {
            interaction_name = name;
            interaction_action_label = action;
            interaction_color = color;
            interaction_point = point;
            interaction_radius = radius;
            interaction_handler = handler;
        }
    }

    private Dictionary<string, InteractionInfo> current_interactions = new Dictionary<string, InteractionInfo>();

    public void resetInteractions()
    {
        current_interactions.Clear();
    }

    public bool addInteraction(InteractionInfo interaction_info)
    {
        if (current_interactions.ContainsKey(interaction_info.interaction_name) == true)
        {
            return false;
        }

        current_interactions.Add(interaction_info.interaction_name, interaction_info);

        return true;
    }

    public bool removeInteraction(string interaction_name)
    {
        if (current_interactions.ContainsKey(interaction_name) == false)
        {
            return false;
        }

        current_interactions.Remove(interaction_name);

        return true;
    }

    public string checkForInteractables(Vector3 player_pos)
    {
        string closest_interaction = "";
        float closest_dist = 9999.9f;

        foreach (KeyValuePair<string, InteractionInfo> kvp in current_interactions)
        {
            float distance_to_interaction_point = Vector3.Distance(player_pos, kvp.Value.interaction_point.position);
            if (distance_to_interaction_point < kvp.Value.interaction_radius && distance_to_interaction_point < closest_dist)
            {
                closest_interaction = kvp.Key;
            }
        }

        return closest_interaction;
    }

    public InteractionInfo getInteractionInfo(string interaction_name)
    {
        return current_interactions[interaction_name];
    }
}