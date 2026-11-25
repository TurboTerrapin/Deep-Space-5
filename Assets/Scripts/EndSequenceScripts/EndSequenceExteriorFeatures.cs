/*
    EndSequenceExteriorFeatures.cs
    - Helps with some of the kinetic elements outside Deep Space Five
    Contributor(s): Jake Schott
    Last Updated: 11/19/2026
*/

using UnityEngine;
using System.Collections;

public class EndSequenceExteriorFeatures : MonoBehaviour
{
    //CLASS CONSTANTS
    private static float VERTICAL_ENERGY_CIRCLES_MOVE_TIME = 2.0f;
    private static float RADAR_SPIN_SPEED = 50.0f;

    public GameObject deep_space_five_white_lights;
    public GameObject deep_space_five_exterior_energy_circles_vertical;
    public GameObject deep_space_five_radar;

    private void Start()
    {
        StartCoroutine(lightBlinker());
        StartCoroutine(energyCircleMover());
    }

    private void Update()
    {
        deep_space_five_radar.transform.Rotate(new Vector3(0.0f, 0.0f, Time.deltaTime * RADAR_SPIN_SPEED));
    }

    IEnumerator lightBlinker()
    {
        while (true)
        {
            deep_space_five_white_lights.GetComponent<Renderer>().material = ReferenceAssistor.Instance.lit_white;
            yield return new WaitForSeconds(ShipExteriorFeatures.LIGHT_BLINK_DELAY);
            deep_space_five_white_lights.GetComponent<Renderer>().material = ReferenceAssistor.Instance.pure_black;
            yield return new WaitForSeconds(ShipExteriorFeatures.LIGHT_BLINK_DELAY);
        }
    }

    IEnumerator energyCircleMover()
    {
        while (true)
        {
            deep_space_five_exterior_energy_circles_vertical.transform.localPosition = Vector3.zero;
            yield return ReferenceAssistor.Instance.translation_helper.linearTranslation(deep_space_five_exterior_energy_circles_vertical.transform, new Vector3(0.0f, 0.0f, 3.5f), VERTICAL_ENERGY_CIRCLES_MOVE_TIME);
        }
    }
}
