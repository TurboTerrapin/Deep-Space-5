

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
    //private BoxCollider shortRangePhaserLeftCollider;
    //private BoxCollider shortRangePhaserRightCollider;

    [Header("LongRange Phaser Settings")]
    [SerializeField] private float minLRBeamWidth = 0.6f;
    [SerializeField] private float maxLRBeamWidth = 3.5f;
    [SerializeField] private float LRBeamEndDiameterRatio = 0.2f;

    [SerializeField] private float baseLRPulseSpeed = 6f;
    [SerializeField] private float maxLRPulseSpeed = 12f;
    [SerializeField] private float LRpulseSmoothing = 0.01f;
    [SerializeField, Range(0.10f, 0.5f)] private float minLRPulsePercentage = 0.1f;
    [SerializeField, Range(0.10f, 0.5f)] private float maxLRPulsePercentage = 0.5f;
    [SerializeField] private float maxLRIntensity = 8.5f;
    [SerializeField] private float LRintensityPulseMultiplier = 2f;

    [Header("ShortRange Phaser Settings")]
    [SerializeField] private float minSRBeamDiameter = 0.1f; // Minimum Short Range Beam Diameter
    [SerializeField] private float maxSRBeamDiameter = 0.3f; // Maximum Short Range Beam Diameter
    [SerializeField] private float SRBeamEndDiameterRatio = 0.4f;
                                                          
    [SerializeField, Range(0.40f, 1f)] private float minSRPulseInterval = 1f; // One Pulse per Unit Time
    [SerializeField, Range(0.40f, 1f)] private float maxSRPulseInterval = 0.4f; // Unit Time / 0.4 = 2.5 Pulse per Unit Time

    // Phaser Materials
    private Material longRangePhaserMaterial;
    private Material shortRangePhaserMaterialLeft;
    private Material shortRangePhaserMaterialRight;
    private Color longRangeEmissionColor;
    private Color shortRangeEmissionColorLeft;
    private Color shortRangeEmissionColorRight;

    private const float LONG_RANGE_BEAM_LENGTH = 500f;
    private const float SHORT_RANGE_BEAM_LENGTH = 150f;
    private const string EMISSION_COLOR = "_EmissionColor";
    private const string EMISSION_ = "_EMISION";

    // Runtime variables
    private float SRPulsePhase;
    private float pulseTimer;
    private float smoothedPulse;
    private float velocity;

    // RunTime Variables
    private bool[] activePhasers; // From PhaserPowers
    private float[] phaserTemps; // From ActivePhasers
    private float longRangePhaserAngle; // From LongRangeDirection


    private void InitializeLongRangePhaser()
    {
        longRangePhaser = longRangePhaserOrigin.GetComponentInChildren<LineRenderer>(true);
        longRangePhaser.useWorldSpace = true;


        if (longRangePhaser != null)
        {
            longRangePhaserCollider = longRangePhaser.GetComponent<BoxCollider>();
            longRangePhaserCollider.isTrigger = true;

            longRangePhaserMaterial = new Material(longRangePhaser.material);
            longRangeEmissionColor = longRangePhaserMaterial.GetColor(EMISSION_COLOR);

            Vector3 startPos = longRangePhaserOrigin.transform.position;
            Vector3 endPos = startPos + longRangePhaserOrigin.transform.forward * LONG_RANGE_BEAM_LENGTH;
            
            // Set Position in worldspace
            longRangePhaser.SetPosition(0, startPos);
                longRangePhaser.SetPosition(1, endPos);
        }
    }
    private void InitializeShortRangePhasers()
    {
        shortRangePhaserLeft = shortRangePhaserLeftOrigin.GetComponentInChildren<LineRenderer>(true);
        if (shortRangePhaserLeft != null)
        {
            shortRangePhaserMaterialLeft = new Material(shortRangePhaserLeft.material);
        }

        shortRangePhaserRight = shortRangePhaserRightOrigin.GetComponentInChildren<LineRenderer>(true);
        if (shortRangePhaserRight != null)
        {
            shortRangePhaserMaterialRight = new Material(shortRangePhaserRight.material);
        }

        shortRangeEmissionColorLeft = shortRangePhaserMaterialLeft.GetColor(EMISSION_COLOR);
        shortRangeEmissionColorRight = shortRangePhaserMaterialRight.GetColor(EMISSION_COLOR);

    }

    private void InitializeTorpedos() { }

    private void Start()
    { 

        InitializeLongRangePhaser();
        InitializeShortRangePhasers();
        InitializeTorpedos();

    }



    public bool AssignControlReferences(GameObject controlHandler) // Called by ShipController.cs
    {
        longRangeDirection = controlHandler.GetComponent<LongRangeDirection>();
        phaserPowers = controlHandler.GetComponent<PhaserPowers>();
        phaserTemperatures = controlHandler.GetComponent<PhaserTemperatures>();

        return longRangeDirection && phaserPowers && phaserTemperatures;
    }

    // **********************************************************************************************

    public void UpdateInput()
    {
        activePhasers = phaserPowers.GetActivePhasers();
        longRangePhaserAngle = longRangeDirection.GetLRPhaserAngle();
        phaserTemps = phaserTemperatures.GetPhaserTemperatures();
    }

   
    private void UpdateLongRangePhaser(float dt)
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

        pulseTimer += dt * temperatureScaledSpeed;
        float currentPulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        smoothedPulse = Mathf.SmoothDamp(smoothedPulse, currentPulse, ref velocity, LRpulseSmoothing);

        float currentBaseWidth = maxLRBeamWidth * beamTemp;
        float pulseWidth = currentBaseWidth * CalculatePulseWidth(beamTemp) * (smoothedPulse * 0.75f);
        float finalWidth = currentBaseWidth + pulseWidth;

        longRangePhaser.startWidth = finalWidth;
        longRangePhaser.endWidth = finalWidth * LRBeamEndDiameterRatio;

        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0, longRangePhaserAngle, 0);
        Vector3 startPos = longRangePhaserOrigin.transform.position;
        Vector3 endPos = startPos + longRangePhaserOrigin.transform.forward * LONG_RANGE_BEAM_LENGTH;
        longRangePhaser.SetPosition(0, startPos);
        longRangePhaser.SetPosition(1, endPos);

        UpdateBeamIntensity(beamTemp, smoothedPulse);
        ResizeCollider(currentBaseWidth);
    }

    private void UpdateShortRangePhaser(LineRenderer phaser, bool active, float temperature, float dt)
    {
        if (phaser == null) return;

        // Handle activation/deactivation
        if (phaser.enabled != active)
        {
            phaser.enabled = active;
            if (!active)
            {
                SRPulsePhase = 0f; // Reset phase when deactivated
                return;
            }
        }

        if (!active)
        {
            phaser.enabled = false;
            SRPulsePhase = 0f;
            return;
        }

        float currentPulseInterval = Mathf.Lerp(maxSRPulseInterval, minSRPulseInterval, 1 - temperature);

        SRPulsePhase += dt / currentPulseInterval;
        SRPulsePhase %= 2f;

        float pulseValue = Mathf.SmoothStep(0, 1, Mathf.PingPong(SRPulsePhase, 1));

        bool beamVisible = pulseValue > 0.3f;
        phaser.enabled = beamVisible;

        float beamWidth = Mathf.Lerp(minSRBeamDiameter, maxSRBeamDiameter, temperature) * pulseValue;
        phaser.startWidth = beamWidth;
        phaser.endWidth = beamWidth * SRBeamEndDiameterRatio;

    }

    private void UpdateShortRangePhasers(float dt)
    {
        bool active = activePhasers[1] && phaserTemps[0] > 0;

        UpdateShortRangePhaser(shortRangePhaserLeft, active, phaserTemps[0], dt); // Handle Left SR Phaser
        UpdateShortRangePhaser(shortRangePhaserRight, active, phaserTemps[0], dt); // Handle Right SR Phaser
    }

    public void UpdateWeapons()
    {
        float dt = Time.deltaTime;
        UpdateLongRangePhaser(dt);
        UpdateShortRangePhasers(dt);
    }

    private float CalculatePulseWidth(float temp)
    {
        return Mathf.Lerp(minLRPulsePercentage, maxLRPulsePercentage, (1f - Mathf.Max(temp, 0.001f)));
    }


    private void UpdateBeamIntensity(float temperature, float pulseIntensity)
    {
        if (!longRangePhaserMaterial) return;

        float intensity = Mathf.Lerp(longRangeEmissionColor.maxColorComponent, maxLRIntensity, temperature)
                        + pulseIntensity;

        longRangePhaserMaterial.EnableKeyword(EMISSION_);
        longRangePhaserMaterial.SetColor(EMISSION_COLOR, longRangeEmissionColor * intensity);
    }

    private void ResizeCollider(float beamWidth)
    {
        if (longRangePhaserCollider == null) return;
        
        // Resize collider
        longRangePhaserCollider.size = new Vector3(
            beamWidth, 
            beamWidth, 
            LONG_RANGE_BEAM_LENGTH
        );
        // Center The collider 
        longRangePhaserCollider.center = new Vector3 (0f, 0f, LONG_RANGE_BEAM_LENGTH * 0.5f);
    }

}

