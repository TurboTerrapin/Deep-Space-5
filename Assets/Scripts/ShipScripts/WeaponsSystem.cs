
using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;
    private LineRenderer longRangePhaser;

    // Beam parameters
    [Header("Long Range Beam Settings")]
    [SerializeField] private float maxBeamWidth = 2.5f; // maximum base diameter 
    [SerializeField] private float basePulseSpeed = 6f; // min cycle speed of pulsing effect
    [SerializeField] private float maxPulseSpeed = 10f;
    [SerializeField] private float pulseSmoothing = 0.1f;
    [SerializeField] private float minPulsePercentage = 0.15f; // pulse width 15% at maxBeamWidth
    [SerializeField] private float maxPulsePercentage = 0.5f; // pulseWidth 50% at minBeamWidth 
    [SerializeField][Range(1f, 10f)] private float minIntensity = 1f;
    [SerializeField][Range(1f, 20f)] private float maxIntensity = 8f;
    [SerializeField] private float intensityPulseMultiplier = 4f;

    [Header("Beam Color Settings")]
    [SerializeField] private float colorPulseMultiplier = 1.5f;

    // Weapon state
    private bool[] activePhasers;
    private float[] phaserTemps;
    private float longRangePhaserAngle;
    private float pulseTimer;
    private float currentPulseValue;
    private float smoothPulseValue;
    private float velocity;
    private Color emissionColor;


    // Weapon origins
    public GameObject longRangePhaserOrigin;
    public GameObject shortRangePhaserOriginLeft;
    public GameObject shortRangePhaserOriginRight;
    public Material longRangePhaserMaterial;

    private void Start()
    {
        InitializeLongRangePhaser();
    }

    private void InitializeLongRangePhaser()
    {
        if (longRangePhaser != null)
        {
            longRangePhaserMaterial = new Material(longRangePhaser.material);
            longRangePhaser.material = longRangePhaserMaterial;
            emissionColor = longRangePhaserMaterial.color;

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
        float temperatureScaledSpeed = Mathf.Lerp(basePulseSpeed, maxPulseSpeed, beamTemp);

        pulseTimer += Time.deltaTime * temperatureScaledSpeed;
        currentPulseValue = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        smoothPulseValue = Mathf.SmoothDamp(smoothPulseValue, currentPulseValue, ref velocity, pulseSmoothing);

        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0f, longRangePhaserAngle, 0f);

        // The currentBaseWidth scales linearly based on beamTemp, (0 -> 1), Where:
        // a beam temperature of 0:0, and a beam temperature of 1:maxBeamSize
        float currentBaseWidth = maxBeamWidth * beamTemp;

        // The pulseWidth scales inversely with the beamTemp
        float pulseWidthPercentage = CalculatePulseWidthPercentage(beamTemp);
        float pulseWidthVariation = currentBaseWidth * pulseWidthPercentage * smoothPulseValue;

        // The finalWidth is the base + the pulse
        float finalWidth = currentBaseWidth + pulseWidthVariation;

        longRangePhaser.startWidth = finalWidth;
        longRangePhaser.endWidth = finalWidth / 10f; // Endpoint of line renderer is 1/10 of the origin diameter (To create perspective)

        // Pulse the Intensity of the HDRI
        UpdateBeamIntensity(beamTemp, smoothPulseValue);
    }

    private float CalculatePulseWidthPercentage(float temperature)
    {
        temperature = Mathf.Max(temperature, 0.001f);
        float inverseTemp = 1f - temperature;
        float curve = inverseTemp * inverseTemp;
        return Mathf.Lerp(minPulsePercentage, maxPulsePercentage, curve);
    }

    private void UpdateBeamIntensity(float temperature, float pulseIntensity)
    {
        if (longRangePhaserMaterial == null) return;

        // Base intensity scales with temperature
        float baseIntensity = Mathf.Lerp(minIntensity, maxIntensity, temperature);

        // Pulse adds intensity variation while maintaining color
        float pulseIntensityBoost = pulseIntensity * intensityPulseMultiplier;
        float totalIntensity = baseIntensity + pulseIntensityBoost;


        // Apply emissive color
        longRangePhaserMaterial.SetColor("_EmissionColor", emissionColor * totalIntensity);
    }

}