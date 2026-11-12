/*
    ScenarioNavigation.cs
    - Handles engineer map
    - Handles engineer heading and target heading
    - Handles hints for boundary issues
    Contributor(s): Jake Schott
    Last Updated: 10/27/2026
*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScenarioNavigation : MonoBehaviour, IPowerable
{
    //CLASS CONSTANTS
    private static float MAP_REFRESH_TIME = 1.0f; //map updates every second
    private static string[] ALTITUDE_WARNING_OPTIONS = new string[] { "DECREASE", "INCREASE" };
    private static string[] ALTITUDE_HINT_OPTIONS = new string[] { "DECREASE ALTITUDE", "INCREASE ALTITUDE" };
    private static float FLASH_SPEED = 0.5f;
    private static Color TMP_RED = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    private static Color SPRITE_RED = new Color(0.96f, 0.25f, 0.28f, 1.0f); //sprite renderer shows color differently
    private static float ITEM_LOCATION_CONVERSION_FACTOR;
    private static float INTEREST_POINT_SIZE_CONVERSION_FACTOR;

    public GameObject navigation_map_display;
    public GameObject navigation_heading_display;
    public List<Sprite> point_of_interest_sprites = null;
    public List<AudioClip> boundary_notifications = null;
    private GameObject detection_warnings;
    private GameObject paths_circle_ships_and_items;
    private GameObject altitude_and_impulse_speed;
    private GameObject altitude_label;
    private GameObject end_sequence_map;
    private GameObject entrance_path;
    private GameObject exit_path;
    private GameObject ship_icon;
    private GameObject points_of_interest;
    private GameObject circle_boundary;
    private GameObject countdown;

    private Coroutine point_of_interest_update_coroutine = null;
    private Coroutine red_flash_coroutine = null;

    public enum PointIconType
    {
        None = 0,
        CollectibleItem = 1,
        DoNotEnter = 2,
        Exclamation = 3,
        Objective = 4
    }

    private void Start()
    {
        ITEM_LOCATION_CONVERSION_FACTOR = (0.265f / (ScenarioManager.BOUNDARY_SIZE * 0.5f));
        INTEREST_POINT_SIZE_CONVERSION_FACTOR = (0.115f / ScenarioManager.BOUNDARY_SIZE);

        detection_warnings = navigation_map_display.transform.GetChild(2).gameObject;
        altitude_and_impulse_speed = navigation_map_display.transform.GetChild(3).gameObject;
        paths_circle_ships_and_items = navigation_map_display.transform.GetChild(4).gameObject;
        end_sequence_map = navigation_map_display.transform.GetChild(5).gameObject;
        entrance_path = paths_circle_ships_and_items.transform.GetChild(0).gameObject;
        exit_path = paths_circle_ships_and_items.transform.GetChild(1).gameObject;
        ship_icon = paths_circle_ships_and_items.transform.GetChild(2).gameObject;
        points_of_interest = paths_circle_ships_and_items.transform.GetChild(3).gameObject;
        circle_boundary = paths_circle_ships_and_items.transform.GetChild(4).gameObject;
        altitude_label = altitude_and_impulse_speed.transform.GetChild(2).gameObject;
        countdown = circle_boundary.transform.GetChild(0).gameObject;

        point_of_interest_update_coroutine = StartCoroutine(pointOfInterestUpdater());
    }

    private void clearPointsOfInterest()
    {
        foreach (Transform t in points_of_interest.transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    private void populatePointsOfInterest(List<MapItem> interest_items)
    {
        for (int i = 0; i < interest_items.Count; i++)
        {
            if (i > points_of_interest.transform.childCount - 1)
            {
                return;
            }

            GameObject item_icon = points_of_interest.transform.GetChild(i).gameObject;

            if (interest_items[i].getInterestType() != PointIconType.CollectibleItem) //not collectible item
            {
                item_icon.GetComponent<SpriteRenderer>().sprite = point_of_interest_sprites[(int)interest_items[i].getInterestType() + 3];
                item_icon.transform.localScale = new Vector3(interest_items[i].getSize() * INTEREST_POINT_SIZE_CONVERSION_FACTOR, interest_items[i].getSize() * INTEREST_POINT_SIZE_CONVERSION_FACTOR, 1.0f);
            }
            else //collectible item
            {
                CollectibleItem ci = interest_items[i].GetComponent<CollectibleItem>();
                if (ci.getItemCategory() == 0) //utility items
                {
                    item_icon.GetComponent<SpriteRenderer>().sprite = point_of_interest_sprites[ci.getItemIndex()];
                }
                else if (ci.getItemCategory() == 1) //torpedoes
                {
                    item_icon.GetComponent<SpriteRenderer>().sprite = point_of_interest_sprites[4];
                }
            }

            Color c = interest_items[i].getColor();
            c.a = 0.2f;
            item_icon.GetComponent<SpriteRenderer>().color = c;

            Vector3 item_position = interest_items[i].transform.localPosition;
            Vector2 item_location = new Vector2(item_position.z - (ScenarioManager.BOUNDARY_SIZE * 0.5f), item_position.x);
            item_location *= ITEM_LOCATION_CONVERSION_FACTOR;
            item_icon.transform.localPosition = new Vector3(item_location.x, item_location.y, 0.0f);

            item_icon.SetActive(true);
        }
    }

    IEnumerator pointOfInterestUpdater()
    {
        while (true)
        {
            GameObject world_root = ReferenceAssistor.Instance.world_root;
            if (world_root != null)
            {
                List<MapItem> map_items = new List<MapItem>();
                foreach (Transform t in world_root.transform)
                {
                    MapItem mi = t.GetComponent<MapItem>();
                    if (mi != null)
                    {
                        if (mi.isVisible() == true)
                        {
                            if (mi.getInterestType() != PointIconType.None)
                            {
                                map_items.Add(mi);
                            }
                        }
                    }
                }

                clearPointsOfInterest();
                populatePointsOfInterest(map_items);
            }

            yield return new WaitForSeconds(MAP_REFRESH_TIME);
        }
    }

    private void colorChange(Color to_change_to)
    {
        circle_boundary.GetComponent<SpriteRenderer>().color = to_change_to;

        GameObject[] paths = new GameObject[2] { entrance_path, exit_path };
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = to_change_to;
            paths[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = to_change_to;
        }
    }

    IEnumerator redLightFlasher()
    {
        countdown.GetComponent<TMP_Text>().color = new Color(TMP_RED.r, TMP_RED.g, TMP_RED.b, 1.0f);
        colorChange(SPRITE_RED);
        while (true)
        {
            for (int i = 0; i < 2; i++)
            {
                float anim_time = FLASH_SPEED;
                while (anim_time > 0.0f)
                {
                    anim_time = Mathf.Max(0.0f, anim_time - Time.deltaTime);
                    if (i == 0)
                    {
                        colorChange(new Color(SPRITE_RED.r, SPRITE_RED.g, SPRITE_RED.b, 1.0f - (0.45f * (1.0f - (anim_time / FLASH_SPEED)))));
                    }
                    else
                    {
                        colorChange(new Color(SPRITE_RED.r, SPRITE_RED.g, SPRITE_RED.b, 0.45f + (0.55f * (1.0f - (anim_time / FLASH_SPEED)))));
                    }
                    yield return null;
                }
            }
        }
    }

    public void updateShipBoundaryStatus(bool inside_boundary, bool within_altitude)
    {
        //update altitude warning
        detection_warnings.transform.GetChild(0).gameObject.SetActive(within_altitude == false);
        if (within_altitude == false)
        {
            altitude_label.GetComponent<TMP_Text>().color = TMP_RED;
            string msg = ALTITUDE_WARNING_OPTIONS[0];
            string hint = ALTITUDE_HINT_OPTIONS[0];
            if (ReferenceAssistor.Instance.world_root.transform.position.y > 0)
            {
                msg = ALTITUDE_WARNING_OPTIONS[1];
                hint = ALTITUDE_HINT_OPTIONS[1];
            }
            ReferenceAssistor.Instance.hints_manager.addHint(hint, 0);
            navigation_map_display.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().SetText("! " + msg + " ALTITUDE !");
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                ReferenceAssistor.Instance.hints_manager.removeHint(ALTITUDE_HINT_OPTIONS[i], 0);
            }
            altitude_label.GetComponent<TMP_Text>().color = new Color(0.0f, 0.84f, 1.0f);
        }

        if (inside_boundary == true)
        {
            if (red_flash_coroutine != null)
            {
                StopCoroutine(red_flash_coroutine);
                red_flash_coroutine = null;
            }

            //color sprite renderers
            colorChange(new Color(0.0f, 0.84f, 1.0f));
            //resize ship pointer
            ship_icon.transform.GetChild(0).localScale = new Vector3(0.01f, 0.01f, 1.0f);
            //color navigation label
            navigation_map_display.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(0.0f, 0.84f, 1.0f, 1.0f);
            //altitude/impulse bars
            altitude_and_impulse_speed.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.84f, 1.0f, 1.0f);
            altitude_and_impulse_speed.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.84f, 1.0f, 1.0f);
            //hide countdown
            countdown.SetActive(false);
            //detection warnings
            detection_warnings.SetActive(false);
        }
        else
        {
            if (red_flash_coroutine == null)
            {
                red_flash_coroutine = StartCoroutine(redLightFlasher());

                //play notification sound(s)
                ReferenceAssistor.Instance.audio_manager.AddNotification(2, boundary_notifications[0]);
                if (within_altitude == true)
                {
                    ReferenceAssistor.Instance.audio_manager.AddNotification(1, boundary_notifications[1]);
                }
                else
                {
                    if (ReferenceAssistor.Instance.world_root.transform.position.y > 0)
                    {
                        ReferenceAssistor.Instance.audio_manager.AddNotification(1, boundary_notifications[2]);
                    }
                    else
                    {
                        ReferenceAssistor.Instance.audio_manager.AddNotification(1, boundary_notifications[3]);
                    }
                }
            }
            //resize ship pointer
            ship_icon.transform.GetChild(0).localScale = new Vector3(0.02f, 0.02f, 1.0f);
            //color navigation label
            navigation_map_display.transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(TMP_RED.r, TMP_RED.g, TMP_RED.b, 1.0f);
            //altitude/impulse bars
            altitude_and_impulse_speed.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = TMP_RED;
            altitude_and_impulse_speed.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().color = TMP_RED;
            //show countdown
            countdown.SetActive(true);
            //detection warnings
            detection_warnings.SetActive(true);
        }
    }

    public void updateShipBoundaryCountdownStatus(int countdown_value)
    {
        if (countdown_value <= 10 && countdown_value > 0)
        {
            ReferenceAssistor.Instance.audio_manager.AddNotification(2, boundary_notifications[3 + countdown_value]);
        }
        countdown.SetActive(true);
        countdown.GetComponent<TMP_Text>().text = countdown_value.ToString();
    }

    public void updatePathLocations(Vector2 ent_path_pos, float ent_rot, Vector2 exit_path_pos, float exit_rot)
    {
        ent_path_pos *= ITEM_LOCATION_CONVERSION_FACTOR;
        exit_path_pos *= ITEM_LOCATION_CONVERSION_FACTOR;
        entrance_path.transform.localPosition = new Vector3(ent_path_pos.x, ent_path_pos.y, 0.0f);
        entrance_path.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -90.0f + ent_rot);
        exit_path.transform.localPosition = new Vector3(exit_path_pos.x, exit_path_pos.y, 0.0f);
        exit_path.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -90.0f + exit_rot);
    }

    //updates ship triangle on map, compass triangle, current heading, and target heading
    public void updateShipOrientation(float ship_rotation, string current_heading, string target_heading)
    {
        ship_icon.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, ship_rotation);

        ship_rotation += 90.0f;
        navigation_heading_display.transform.GetChild(1).GetChild(0).transform.localRotation = Quaternion.Euler(0.0f, 180.0f, ship_rotation);

        navigation_heading_display.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>().SetText(current_heading);
        navigation_heading_display.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().SetText(target_heading);
    }

    //updates ship triangle location on map
    public void updateShipLocation(Vector3 world_root_position)
    {
        Vector2 ship_location = new Vector2(-world_root_position.z - (ScenarioManager.BOUNDARY_SIZE * 0.5f), -world_root_position.x);
        ship_location *= ITEM_LOCATION_CONVERSION_FACTOR;
        if (ship_location.y <= -0.285f)
        {
            ship_icon.transform.GetChild(0).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }
        else
        {
            ship_icon.transform.GetChild(0).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
        }
        ship_icon.transform.localPosition = new Vector3(ship_location.x, ship_location.y, 0.0f);
    }


    public void updateAltitude(float current_altitude)
    {
        string rounded_altitude = (Mathf.Round(current_altitude * 10.0f) / 10.0f).ToString();
        if (rounded_altitude.Contains(".") == false)
        {
            rounded_altitude += ".0";
        }
        altitude_label.GetComponent<TMP_Text>().SetText("ALTITUDE: " + rounded_altitude + "m");
    }

    public void displayEndSequence()
    {
        if (point_of_interest_update_coroutine != null)
        {
            StopCoroutine(point_of_interest_update_coroutine);
            point_of_interest_update_coroutine = null;
        }
        updateShipBoundaryStatus(true, true);
        clearPointsOfInterest();
        detection_warnings.SetActive(false);
        paths_circle_ships_and_items.SetActive(false);
        altitude_and_impulse_speed.SetActive(false);
        end_sequence_map.SetActive(true);
        updateShipOrientation(0.0f, "90.0°", "");
    }

    public void powerOn(int position)
    {
        if (navigation_map_display.activeSelf == true)
        {
            navigation_heading_display.SetActive(true);
        }
        navigation_map_display.SetActive(true);
    }

    public void powerOff(int position, float time)
    {
        navigation_map_display.SetActive(false);
        navigation_heading_display.SetActive(false);
    }
}