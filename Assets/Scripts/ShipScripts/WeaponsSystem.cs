using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;
    private LineRenderer longRangePhaser;

    // Beam parameters
    [Header("Beam Settings")]
    [SerializeField] private float maxBeamWidth = 2.5f;
    [SerializeField] private float basePulseSpeed = 6f;
    [SerializeField] private float maxPulseSpeed = 10f; // Maximum speed at high temp
    [SerializeField] private float maxPulseAmplitude = 0.2f;
    [SerializeField] private float pulseSmoothing = 0.1f;

    // Weapon state
    private bool[] activePhasers;
    private float[] phaserTemps;
    private float longRangePhaserAngle;
    private float pulseTimer;
    private float currentPulseValue;
    private float smoothPulseValue;
    private float velocity;

    // Weapon origins
    public GameObject longRangePhaserOrigin;

    private void Start()
    {
        InitializeLongRangePhaser();
    }

    private void InitializeLongRangePhaser()
    {
        if (longRangePhaser != null)
        {
            longRangePhaser.material = new Material(longRangePhaser.material);
        }
    }

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
    }

    private void UpdateLongRangePhaser()
    {
        bool active = activePhasers[0] && (phaserTemps[1] > 0);

        if (longRangePhaser.enabled != active)
        {
            longRangePhaser.enabled = active;
            if (!active) pulseTimer = 0f;
        }

        if (!longRangePhaser.enabled) return;

        float beamTemp = Mathf.Clamp01(phaserTemps[1]);
        float temperatureScaledAmplitude = maxPulseAmplitude * beamTemp;
        float temperatureScaledSpeed = Mathf.Lerp(basePulseSpeed, maxPulseSpeed, beamTemp);

        pulseTimer += Time.deltaTime * temperatureScaledSpeed;
        currentPulseValue = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        smoothPulseValue = Mathf.SmoothDamp(smoothPulseValue, currentPulseValue, ref velocity, pulseSmoothing);

        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0f, longRangePhaserAngle, 0f);

        float baseWidth = Mathf.Lerp(0f, maxBeamWidth, beamTemp);
        float pulseWidth = baseWidth * (1f + smoothPulseValue * temperatureScaledAmplitude);

        longRangePhaser.startWidth = pulseWidth;
        longRangePhaser.endWidth = pulseWidth;
    }
}