/*
    ScanWaveManager.cs
    - Handles scan waves for ship, probe, and tractor beam
    Contributor(s): Jake Schott
    Last Updated: 6/8/2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanWaveManager : MonoBehaviour
{
    public List<GameObject> waves = null; //ship, probe, and tractor beam
    public WaveInfo[] wave_information = new WaveInfo[3] { new WaveInfo(), new WaveInfo(), new WaveInfo() };

    public void initializeWave(int index, WaveInfo wi)
    {
        waves[index].GetComponent<ScanWave>().resetWave();
        wave_information[index] = wi;
        waves[index].GetComponent<ScanWave>().initializeWave(wave_information[index]);
    }

    public void updateWave(int index)
    {
        waves[index].GetComponent<ScanWave>().updateWave();
    }

    public WaveInfo getWaveInfo(int index)
    {
        return wave_information[index];
    }
}