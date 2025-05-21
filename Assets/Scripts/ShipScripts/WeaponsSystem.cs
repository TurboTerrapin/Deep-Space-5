using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;
    private LineRenderer longRangePhaser;

    [Header("Beam Settings")]
    [SerializeField] private float maxBeamWidth = 2.5f;
    [SerializeField] private float basePulseSpeed = 6f;
    [SerializeField] private float maxPulseSpeed = 10f;
    [SerializeField] private float pulseSmoothing = 0.1f;
    [SerializeField, Range(0.15f, 0.5f)] private float minPulsePercentage = 0.15f;
    [SerializeField, Range(0.15f, 0.5f)] private float maxPulsePercentage = 0.5f;
    [SerializeField] private float maxIntensity = 6f;
    [SerializeField] private float intensityPulseMultiplier = 4f;

    [Header("References")]
    public GameObject longRangePhaserOrigin;
    public Material longRangePhaserMaterial;
    public Color emissionColor;

    // Runtime variables
    private bool[] activePhasers;
    private float[] phaserTemps;
    private float longRangePhaserAngle;
    private float pulseTimer;
    private float smoothPulseValue;
    private float velocity;

    private void Start() => InitializeLongRangePhaser();

    private void InitializeLongRangePhaser()
    {
        if (longRangePhaser != null)
        {
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
        
        if (longRangePhaserOrigin)
            longRangePhaser = longRangePhaserOrigin.GetComponentInChildren<LineRenderer>(true);

        return longRangeDirection && phaserPowers && phaserTemperatures && longRangePhaser;
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
            if (!active) pulseTimer = 0f;
            return;
        }

        float beamTemp = Mathf.Clamp01(phaserTemps[1]);
        float temperatureScaledSpeed = Mathf.Lerp(basePulseSpeed, maxPulseSpeed, beamTemp);

        pulseTimer += Time.deltaTime * temperatureScaledSpeed;
        float currentPulseValue = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        smoothPulseValue = Mathf.SmoothDamp(smoothPulseValue, currentPulseValue, ref velocity, pulseSmoothing);

        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0f, longRangePhaserAngle, 0f);

        float currentBaseWidth = maxBeamWidth * beamTemp;
        float pulseWidth = currentBaseWidth * CalculatePulseWidth(beamTemp) * smoothPulseValue;
        float finalWidth = currentBaseWidth + pulseWidth;

        longRangePhaser.startWidth = finalWidth;
        longRangePhaser.endWidth = finalWidth * 0.1f;

        UpdateBeamIntensity(beamTemp, smoothPulseValue);
    }

    private float CalculatePulseWidth(float temp) => 
        Mathf.Lerp(minPulsePercentage, maxPulsePercentage, (1f - Mathf.Max(temp, 0.001f)));

    private void UpdateBeamIntensity(float temperature, float pulseIntensity)
    {
        if (!longRangePhaserMaterial) return;
        
        float intensity = Mathf.Lerp(emissionColor.maxColorComponent, maxIntensity, temperature) 
                       + pulseIntensity * intensityPulseMultiplier;

        longRangePhaserMaterial.EnableKeyword("_EMISSION");
        longRangePhaserMaterial.SetColor("_EmissionColor", emissionColor * intensity);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(longRangePhaserMaterial);
        #endif
    }
}