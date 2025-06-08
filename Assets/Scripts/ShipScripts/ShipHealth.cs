using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShipHealth : NetworkBehaviour
{
    //CLASS CONSTANTS
    private static float UPDATE_TIME = 1.0f;
    private static Color MAX_HEALTH = new Color(0.0f, 1.0f, 0.11f, 0.22f);
    private static Color ZERO_HEALTH = new Color(1.0f, 0.09f, 0.0f, 0.52f);

    public List<GameObject> ship_health_indicators = null;

    private float[] health_areas = new float[4] { 100.0f, 100.0f, 100.0f, 100.0f }; //corresponds to forward, port, starboard, aft
    private Coroutine damage_animation_coroutine = null;

    private void Start()
    {
        StartCoroutine(tester());
    }

    IEnumerator tester()
    {
        yield return new WaitForSeconds(3.0f);
        for (int i = 0; i < 20; i++)
        {
            damageAllSections(15.0f);
            yield return new WaitForSeconds(2.0f);
        }
    }

    IEnumerator showDamageEffects()
    {
        Color[] start_colors = new Color[4];
        Color[] desired_colors = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            start_colors[i] = ship_health_indicators[i].GetComponent<UnityEngine.UI.RawImage>().color;
            desired_colors[i] =
                new Color(Mathf.Lerp(ZERO_HEALTH.r, MAX_HEALTH.r, health_areas[i] / 100.0f),
                          Mathf.Lerp(ZERO_HEALTH.g, MAX_HEALTH.g, health_areas[i] / 100.0f),
                          Mathf.Lerp(ZERO_HEALTH.b, MAX_HEALTH.b, health_areas[i] / 100.0f),
                          Mathf.Lerp(ZERO_HEALTH.a, MAX_HEALTH.a, health_areas[i] / 100.0f));
        }

        float animation_time = UPDATE_TIME;

        while (animation_time > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            animation_time = Mathf.Max(0.0f, animation_time - dt);

            for (int i = 0; i < 4; i++)
            {
                ship_health_indicators[i].GetComponent<UnityEngine.UI.RawImage>().color =
                    new Color(Mathf.Lerp(desired_colors[i].r, start_colors[i].r, animation_time / UPDATE_TIME),
                              Mathf.Lerp(desired_colors[i].g, start_colors[i].g, animation_time / UPDATE_TIME),
                              Mathf.Lerp(desired_colors[i].b, start_colors[i].b, animation_time / UPDATE_TIME),
                              Mathf.Lerp(desired_colors[i].a, start_colors[i].a, animation_time / UPDATE_TIME));
            }

            yield return null;
        }

        damage_animation_coroutine = null;
    }

    //helper function that subtracts damage and rounds
    private void updateHealth(float dam, int index)
    {
        float updated_health = Mathf.Max(0.0f, health_areas[index] - dam);
        updated_health = Mathf.Round(updated_health * 10.0f) / 10.0f;
        health_areas[index] = updated_health;
    }

    public void damageSection(float damage, int section)
    {
        updateHealth(damage, section);
        transmitHealthChangeRPC(health_areas[0], health_areas[1], health_areas[2], health_areas[3]);
    }

    //will damage every section randomly between half and full damage but ensure that one is damaged as much as inputted parameter
    public void damageAllSections(float damage)
    {
        int most_damaged_area = Random.Range(0, 4);
        updateHealth(damage, most_damaged_area);
        for (int i = 0; i < 4; i++)
        {
            if (i != most_damaged_area)
            {
                updateHealth(Random.Range(damage * 0.5f, damage), i);
            }
        }
        transmitHealthChangeRPC(health_areas[0], health_areas[1], health_areas[2], health_areas[3]);
    }

    [Rpc(SendTo.Everyone)]
    private void transmitHealthChangeRPC(float fwd_health, float port_health, float stbd_health, float aft_health)
    {
        health_areas[0] = fwd_health;
        health_areas[1] = port_health;
        health_areas[2] = stbd_health;
        health_areas[3] = aft_health;
        if (damage_animation_coroutine != null)
        {
            StopCoroutine(damage_animation_coroutine);
        }
        damage_animation_coroutine = StartCoroutine(showDamageEffects());
    }
}
