

/*
Handles Long-Range & Short-Range Phasers
*/

using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;
    private LineRenderer longRangePhaser;

    private BoxCollider longRangePhaserCollider;

    [Header("Beam Settings")]
    [SerializeField] private float maxBeamWidth = 1.5f;
    [SerializeField] private float basePulseSpeed = 6f;
    [SerializeField] private float maxPulseSpeed = 12f;
    [SerializeField] private float pulseSmoothing = 0.1f;
    [SerializeField, Range(0.10f, 0.5f)] private float minPulsePercentage = 0.075f;
    [SerializeField, Range(0.10f, 0.5f)] private float maxPulsePercentage = 0.5f;
    [SerializeField] private float maxIntensity = 6.5f;
    [SerializeField] private float intensityPulseMultiplier = 2f;

    [Header("References")]
    public GameObject longRangePhaserOrigin;
    public Material longRangePhaserMaterial;
    public Color emissionColor;

    // Runtime variables

    private float beamLength = 500f;
    private bool[] activePhasers;
    private float[] phaserTemps;
    private float longRangePhaserAngle;
    private float pulseTimer;
    private float smoothedPulse;
    private float velocity;

    private void Start() => InitializeLongRangePhaser();

    private void InitializeLongRangePhaser()
    {
        longRangePhaser = longRangePhaserOrigin.GetComponentInChildren<LineRenderer>(true);

        if (longRangePhaser != null)
        {
            longRangePhaserCollider = longRangePhaser.GetComponent<BoxCollider>();
            longRangePhaserCollider.isTrigger = true;

            longRangePhaserMaterial = new Material(longRangePhaser.material);
            longRangePhaser.material = longRangePhaserMaterial;
            emissionColor = longRangePhaserMaterial.GetColor("_EmissionColor");
        }
    }

    public bool AssignControlReferences(GameObject controlHandler)
    {
        longRangeDirection = controlHandler.GetComponent<LongRangeDirection>();
        phaserPowers = controlHandler.GetComponent<PhaserPowers>();
        phaserTemperatures = controlHandler.GetComponent<PhaserTemperatures>();

        return longRangeDirection && phaserPowers && phaserTemperatures;
    }

    public void UpdateInput()
    {
        activePhasers = phaserPowers.GetActivePhasers();
        longRangePhaserAngle = longRangeDirection.GetLRPhaserAngle();
        phaserTemps = phaserTemperatures.GetPhaserTemperatures();
    }

    public void UpdateWeapons() => UpdateLongRangePhaser();

    private void UpdateLongRangePhaser()
    {
        bool active = activePhasers[0] && phaserTemps[1] > 0;

        if (longRangePhaser.enabled != active)
        {
            longRangePhaser.enabled = active;
            if (longRangePhaserCollider != null) longRangePhaserCollider.enabled = active;
            if (!active) pulseTimer = 0f;
            return;
        }

        float beamTemp = Mathf.Clamp01(phaserTemps[1]);
        float temperatureScaledSpeed = Mathf.Lerp(basePulseSpeed, maxPulseSpeed, beamTemp);

        pulseTimer += Time.deltaTime * temperatureScaledSpeed;
        float currentPulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        smoothedPulse = Mathf.SmoothDamp(smoothedPulse, currentPulse, ref velocity, pulseSmoothing);

        float currentBaseWidth = maxBeamWidth * beamTemp;
        float pulseWidth = currentBaseWidth * CalculatePulseWidth(beamTemp) * (smoothedPulse * 0.75f);
        float finalWidth = currentBaseWidth + pulseWidth;

        longRangePhaser.startWidth = finalWidth;
        longRangePhaser.endWidth = finalWidth * 0.2f;

        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0, longRangePhaserAngle, 0);
        UpdateBeamIntensity(beamTemp, -smoothedPulse);
        UpdateCollider(currentBaseWidth);
    }

    private float CalculatePulseWidth(float temp) =>
        Mathf.Lerp(minPulsePercentage, maxPulsePercentage, (1f - Mathf.Max(temp, 0.001f)));

    private void UpdateBeamIntensity(float temperature, float pulseIntensity)
    {
        if (!longRangePhaserMaterial) return;

        float intensity = Mathf.Lerp(emissionColor.maxColorComponent, maxIntensity, temperature)
                        + pulseIntensity;

        longRangePhaserMaterial.EnableKeyword("_EMISSION");
        longRangePhaserMaterial.SetColor("_EmissionColor", emissionColor * intensity);
    }
    private void UpdateCollider(float beamWidth)
    {
        if (longRangePhaserCollider == null) return;
        
        // Resize collider
        longRangePhaserCollider.size = new Vector3(
            beamWidth, 
            beamWidth, 
            beamLength 
        );
        // Center Collider
        longRangePhaserCollider.center = new Vector3(0, 0, beamLength * 0.5f);
    }

}