using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    // References
    private LongRangeDirection longRangeDirection;
    private PhaserPowers phaserPowers;
    private PhaserTemperatures phaserTemperatures;
    private LineRenderer longRangePhaser;

    // Weapon parameters
    private readonly float maxBeamWidth = 3.5f;

    // Weapon state
    private bool[] activePhasers;
    private float[] phaserTemps;
    private float longRangePhaserAngle;

    // Weapon origins
    public GameObject longRangePhaserOrigin;
    public GameObject shortRangePhaserLeftOrigin;
    public GameObject shortRangePhaserRightOrigin;

    public Material longRangeMaterial;
    public Color longRangeBaseColor;
    public Color longRangeHDRemission;
    public float baseEmissionIntensity;
    public float pulseTimer;
    public float pulseSpeed;
    public float pulseWidthAmplitude = 1f;

    private void Start()
    {
        InitializeLongRangePhasers();
    }

    private void InitializeLongRangePhasers()
    {
        longRangeMaterial = new Material(longRangePhaser.material);
        longRangePhaser.material = longRangeMaterial;

        
        longRangeBaseColor = longRangeMaterial.color;
        longRangeHDRemission = longRangeMaterial.GetColor("_EmissionColor");
        baseEmissionIntensity = longRangeHDRemission.maxColorComponent;
        longRangeHDRemission /= baseEmissionIntensity; // Normalize


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
        // UpdateShortRangePhasers(); // Implement when ready
    }

    private void UpdateLongRangePhaser()
    {
        bool active = activePhasers[0] && (phaserTemps[1] > 0);

        if (longRangePhaser.enabled != active)
        {
            longRangePhaser.enabled = active;

        }

        if (!active)
        {
            // Reset timer when beam becomes inactive
            pulseTimer = 0f;
            return;
        }

        // Update Pulse Timer
        pulseTimer += Time.deltaTime * pulseSpeed;

        // Calculate Pulse factotr
        float pulseFactor = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // Sin Wave -> (-1, 1) to (0, 1)

        // Rotate Beam
        longRangePhaserOrigin.transform.localRotation = Quaternion.Euler(0f, longRangePhaserAngle, 0f);

        // Adjust beam width based on temperature
        float beamTemp = Mathf.Clamp01(phaserTemps[1]);
        float pulseWidth = Mathf.Lerp(0f, maxBeamWidth, beamTemp * pulseFactor);
        //float pulseWidth = baseBeamWidth * pulseWidthAmplitude;

        longRangePhaser.startWidth = pulseWidth;
        longRangePhaser.endWidth = pulseWidth;

    
    }

}