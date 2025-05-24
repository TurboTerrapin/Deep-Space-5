

/*
Handles Long-Range & Short-Range Phasers
*/

using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // Tactican Control References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;

    // LineRenderers (Phasers)
    private LineRenderer longRangePhaser;
    private LineRenderer shortRangePhaserLeft;
    private LineRenderer shortRangePhaserRight;

    // LineRenderer Origins
    public GameObject longRangePhaserOrigin;
    public GameObject shortRangePhaserLeftOrigin;
    public GameObject shortRangePhaserRightOrigin;

    // Colliders (Phasers)
    private BoxCollider longRangePhaserCollider;
    private BoxCollider shortRangePhaserLeftCollider;
    private BoxCollider shortRangePhaserRightCollider;

    [Header("LongRange Phaser Settings")]
    [SerializeField] private float maxLRBeamWidth = 1.5f;
    [SerializeField] private float baseLRPulseSpeed = 6f;
    [SerializeField] private float maxLRPulseSpeed = 12f;
    [SerializeField] private float LRpulseSmoothing = 0.01f;
    [SerializeField, Range(0.10f, 0.5f)] private float minLRPulsePercentage = 0.1f;
    [SerializeField, Range(0.10f, 0.5f)] private float maxLRPulsePercentage = 0.5f;
    [SerializeField] private float maxLRIntensity = 6.5f;
    [SerializeField] private float LRintensityPulseMultiplier = 2f;

    [Header("ShortRange Phaser Settings")]
    [SerializeField] private float maxSRBeamWidth = 0.75f;

    // Phaser Materials
    private Material longRangePhaserMaterial;
    private Material shortRangePhaserMaterial;
    private Color longRangeEmissionColor;
    private Color shortRangeEmissionColor;



    // Runtime variables

    private float longRangeBeamLength = 500f;


    private float pulseTimer;
    private float smoothedPulse;
    private float velocity;

    // RunTime Variables
    private bool[] activePhasers; // From PhaserPowers
    private float[] phaserTemps; // From ActivePhasers
    private float longRangePhaserAngle; // From LongRangeDirection

    private void Start()
    {
        InitializeLongRangePhaser();
        InitializeShortRangePhasers();

    }

    private void InitializeLongRangePhaser()
    {
        longRangePhaser = longRangePhaserOrigin.GetComponentInChildren<LineRenderer>(true);

        if (longRangePhaser != null)
        {
            longRangePhaserCollider = longRangePhaser.GetComponent<BoxCollider>();
            longRangePhaserCollider.isTrigger = true;

            longRangePhaserMaterial = new Material(longRangePhaser.material);
            longRangePhaser.material = longRangePhaserMaterial;
            longRangeEmissionColor = longRangePhaserMaterial.GetColor("_EmissionColor");
        }
    }
    private void InitializeShortRangePhasers()
    {
        shortRangePhaserLeft = shortRangePhaserLeftOrigin.GetComponentInChildren<LineRenderer>(true);
        if (shortRangePhaserLeft != null)
        {
            shortRangePhaserLeft.material = new Material(shortRangePhaserLeft.material);
        }

        shortRangePhaserRight = shortRangePhaserRightOrigin.GetComponentInChildren<LineRenderer>(true);
        if (shortRangePhaserRight != null)
        {
            shortRangePhaserRight.material = new Material(shortRangePhaserRight.material);
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

    public void UpdateWeapons()
    {
        UpdateLongRangePhaser();
        UpdateShortRangePhasers();
    }

    private void UpdateShortRangePhasers()
    {
        bool active = activePhasers[1] && phaserTemps[0] > 0;

        UpdateSingleShortRangePhaser(shortRangePhaserLeft, active, phaserTemps[0]);
        UpdateSingleShortRangePhaser(shortRangePhaserRight, active, phaserTemps[0]);
    }

    /*

    private void UpdateSingleShortRangePhaser(LineRenderer phaser, bool active, float temperature)
    {
        if (phaser == null) return;

        if (phaser.enabled != active)
        {
            phaser.enabled = active;
            if (!active) return;
        }

        if (!active)
        {
            phaser.enabled = false;
            return;
        }

        float minPulseInterval = 0.05f; 
        float maxPulseInterval = 0.5f; 
        float pulseInterval = Mathf.Lerp(minPulseInterval, maxPulseInterval, 1 - temperature);

        float pulseValue = Mathf.PingPong(Time.time / pulseInterval, 1);

        bool beamVisible = pulseValue > 0.5f;
        phaser.enabled = beamVisible;

        if (beamVisible)
        {
            float beamWidth = Mathf.Lerp(0.5f, maxLRBeamWidth * 0.75f, temperature);
            phaser.startWidth = beamWidth;
            phaser.endWidth = beamWidth * 0.2f;

        }
    }
    */


    private void UpdateSingleShortRangePhaser(LineRenderer phaser, bool active, float temperature)
    {
        if (phaser == null) return;

        if (phaser.enabled != active)
        {
            phaser.enabled = active;
            if (!active) return;
        }

        if (!active)
        {
            phaser.enabled = false;
            return;
        }



        // Define base pulse parameters
        float minPulseInterval = 0.15f;  // Slower minimum pulse
        float maxPulseInterval = 0.5f;   // Same maximum pulse

        // Calculate pulse interval - higher temp = longer interval (slower pulse)
        float pulseInterval = Mathf.Lerp(minPulseInterval, maxPulseInterval, 1f);

        // Calculate pulse value using smooth ping-pong
        float pulseValue = Mathf.SmoothStep(0, 1, Mathf.PingPong(Time.time / pulseInterval, 1));

        // Make beam visible for a longer portion of the pulse cycle
        bool beamVisible = pulseValue > 0.3f;  // More visible time
        phaser.enabled = beamVisible;

        if (beamVisible)
        {
            // Smoother width transition
            float beamWidth = Mathf.Lerp(0.5f, maxSRBeamWidth, temperature) * pulseValue;
            phaser.startWidth = beamWidth;
            phaser.endWidth = beamWidth * 0.2f;


            // Optional: Add slight emission intensity variation
            if (phaser.material.HasProperty("_EmissionColor"))
            {
                //Color emissionColor = phaser.material.GetColor("_EmissionColor");
                //phaser.material.SetColor("_EmissionColor", emissionColor * (0.8f + 0.4f * pulseValue));
            }
        }
    }

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
        float temperatureScaledSpeed = Mathf.Lerp(baseLRPulseSpeed, maxLRPulseSpeed, beamTemp);

        pulseTimer += Time.deltaTime * temperatureScaledSpeed;
        float currentPulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        smoothedPulse = Mathf.SmoothDamp(smoothedPulse, currentPulse, ref velocity, LRpulseSmoothing);

        float currentBaseWidth = maxLRBeamWidth * beamTemp;
        float pulseWidth = currentBaseWidth * CalculatePulseWidth(beamTemp) * (smoothedPulse * 0.75f);
        float finalWidth = currentBaseWidth + pulseWidth;

        longRangePhaser.startWidth = finalWidth;
        longRangePhaser.endWidth = finalWidth * 0.2f;

        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0, longRangePhaserAngle, 0);
        UpdateBeamIntensity(beamTemp, smoothedPulse);
        UpdateCollider(currentBaseWidth, maxLRBeamWidth);
    }

    private float CalculatePulseWidth(float temp) =>
        Mathf.Lerp(minLRPulsePercentage, maxLRPulsePercentage, (1f - Mathf.Max(temp, 0.001f)));

    private void UpdateBeamIntensity(float temperature, float pulseIntensity)
    {
        if (!longRangePhaserMaterial) return;

        float intensity = Mathf.Lerp(longRangeEmissionColor.maxColorComponent, maxLRIntensity, temperature)
                        + pulseIntensity;

        longRangePhaserMaterial.EnableKeyword("_EMISSION");
        longRangePhaserMaterial.SetColor("_EmissionColor", longRangeEmissionColor * intensity);
    }
    private void UpdateCollider(float beamWidth, float beamLength)
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

    // Include on Collision detection .
    // draw distance (coord2) to impact point on box collider (Redraw line renderer between origin and impact)

}

