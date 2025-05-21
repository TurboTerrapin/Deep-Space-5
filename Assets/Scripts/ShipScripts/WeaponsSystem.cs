using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;
    private LineRenderer longRangePhaser;

    // Weapon parameters
    private readonly float maxBeamWidth = 1.5f;

    // Weapon state
    private bool[] activePhasers;
    private float[] phaserTemps;
    private float longRangePhaserAngle;

    // Weapon origins
    public GameObject longRangePhaserOrigin;
    public GameObject shortRangePhaserLeftOrigin;
    public GameObject shortRangePhaserRightOrigin;

    public bool AssignControlReferences(GameObject controlHandler)
    {
        longRangeDirection = controlHandler.GetComponent<LongRangeDirection>();
        phaserPowers = controlHandler.GetComponent<PhaserPowers>();
        phaserTemperatures = controlHandler.GetComponent<PhaserTemperatures>();

        if (longRangePhaserOrigin != null)
        {
            longRangePhaser = longRangePhaserOrigin.GetComponentInChildren<LineRenderer>(true);
        }

        return longRangeDirection && phaserPowers &&
               phaserTemperatures && longRangePhaser != null;
    }

    public void UpdateInput()
    {
        activePhasers = phaserPowers.GetActivePhasers();
        longRangePhaserAngle = longRangeDirection.GetLRPhaserAngle();
        phaserTemps = phaserTemperatures.GetPhaserTemperatures();
    }

    public void UpdateWeapons()
    {
        UpdateLongRangePhaser();
        // UpdateShortRangePhasers(); // Implement when ready
    }

    private void UpdateLongRangePhaser()
    {
        bool shouldBeActive = activePhasers[0] && (phaserTemps[1] > 0);

        if (longRangePhaser.enabled != shouldBeActive)
        {
            longRangePhaser.enabled = shouldBeActive;
        }

        if (!shouldBeActive) return;

        // Rotate Beam
        longRangePhaserOrigin.transform.localRotation =
            Quaternion.Euler(0f, longRangePhaserAngle, 0f);

        // Adjust beam width based on temperature
        float beamTemp = Mathf.Clamp01(phaserTemps[1]);
        float beamWidth = Mathf.Lerp(0f, maxBeamWidth, beamTemp);
        longRangePhaser.startWidth = beamWidth;
        longRangePhaser.endWidth = beamWidth;
    }

}