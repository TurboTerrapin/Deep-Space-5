/*
    ScanWave.cs
    - Handles visuals for a scan wave as seen in the engineer position
    - Currently operates under the assumption that there are only four rings
    Contributor(s): Jake Schott
    Last Updated: 6/8/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanWave : MonoBehaviour
{
    //CLASS CONSTANTS
    private static int NUM_OF_ITEMS_PER_RING = 36;

    public GameObject wave_center;
    public List<GameObject> rings = null; //a-d
    private Coroutine default_rotation_coroutine = null;

    IEnumerator spinRings()
    {
        while (true)
        {
            yield return null;
        }
    }

    public void resetWave()
    {
        //destroy all individual items
        for (int r = 0; r < rings.Count; r++)
        {
            for (int x = 1; x < rings[r].transform.GetChild(0).childCount - 1; x++)
            {
                Destroy(rings[r].transform.GetChild(1).gameObject);
            }
        }

        //stop spinning
        if (default_rotation_coroutine != null)
        {
            StopCoroutine(default_rotation_coroutine);
        }
        default_rotation_coroutine = null;
    }

    public void initializeWave(WaveInfo wave_info)
    {
        //set center
        wave_center.GetComponent<UnityEngine.UI.RawImage>().texture = wave_info.getCenterTexture();
        wave_center.GetComponent<UnityEngine.UI.RawImage>().color = wave_info.getCenterColor();

        //set rings
        for (int r = 0; r < wave_info.getNumberOfRings(); r++)
        {
            if (wave_info.getRingSolids()[r] == true) //is just a texture that spins around
            {
                rings[r].GetComponent<UnityEngine.UI.RawImage>().texture = wave_info.getRingTextures()[r];
                rings[r].GetComponent<UnityEngine.UI.RawImage>().color = wave_info.getRingColors()[r];
            }
            else //is comprised of more than one textures (ex. circles, diamonds)
            {
                //hide the outer ring
                rings[r].GetComponent<UnityEngine.UI.RawImage>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                //create ring items
                for (int i = 0; i < NUM_OF_ITEMS_PER_RING; i++)
                {
                    GameObject new_item = Object.Instantiate(rings[r].transform.GetChild(0).GetChild(0).gameObject, rings[r].transform.GetChild(0));
                    new_item.GetComponent<UnityEngine.UI.RawImage>().texture = wave_info.getRingTextures()[r];
                    new_item.GetComponent<UnityEngine.UI.RawImage>().color = wave_info.getRingColors()[r];
                    new_item.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, i * (360.0f / NUM_OF_ITEMS_PER_RING));
                }
            }
        }
        default_rotation_coroutine = StartCoroutine(spinRings());
    }

    public void updateWave()
    {

    }

    public void contractWave()
    {

    }

    public void expandWave()
    {

    }

    public void changeWaveSpeed()
    {

    }
}