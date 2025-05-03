using UnityEngine;
public class ShipController : MonoBehaviour
{
    // Object References
    private GameObject controlHandler;
    public GameObject bridge;

    // Pilot Script References
    private ImpulseThrottle impulseThrottle;
    private CourseHeading courseHeading;
    private HorizontalThrusters horizontalThrusters;
    private VerticalThrusters verticalThrusters;

    public float currentImpulse;
    public float currentHeading;
    public float horizontalThrust;
    public float verticalThrust;

    private bool shipReady = false;

    private bool AssignPilotControlRefs()
    {
        impulseThrottle = controlHandler.GetComponent<ImpulseThrottle>();
        courseHeading = controlHandler.GetComponent<CourseHeading>();
        horizontalThrusters = controlHandler.GetComponent<HorizontalThrusters>();
        verticalThrusters = controlHandler.GetComponent<VerticalThrusters>();

        return impulseThrottle && courseHeading &&
                horizontalThrusters && verticalThrusters != null;
    }
    private bool AssignTacticianControlRefs() { return true; }
    private bool AssignEngineerControlRefs() { return true; }
    private bool AssignCaptainControlRefs() { return true; }

    void Start()
    {
        controlHandler = GameObject.FindGameObjectWithTag("ControlHandler");

        if (controlHandler != null && AssignPilotControlRefs() && AssignTacticianControlRefs() &&
                AssignEngineerControlRefs() && AssignCaptainControlRefs())
        {
            shipReady = true;
        }
    }

    private void GetPilotInput()
    {
        currentImpulse = impulseThrottle.getCurrentImpulse();
        currentHeading = courseHeading.getCurrentHeading();
        horizontalThrust = horizontalThrusters.getHorizontalThrust();
        verticalThrust = verticalThrusters.getVerticalThrust();
    }
    private void GetTacticianInput() { }
    private void GetEngineerInput() { }
    private void GetCaptainInput() { }

    void Update()
    {
        if (shipReady)
        {
            UpdateBridgeTransform();
            GetPilotInput();
            GetTacticianInput();
            GetEngineerInput();
            GetCaptainInput();
            
            UpdateShipTransform();
        }
    }

    // *********** To Separate into own script in future ****************8

    private readonly float maxThrusterSpeed = 2f;
    private readonly float maxImpulseSpeed = 25f;
    private readonly float rotationPower = 6f; //60

    private readonly float impulseAccelerationRate = 0.4f;
    private readonly float impulseDecelerationRate = 1.5f;

    private readonly float baseThrusterAccelerationRate = 0.1f;
    private readonly float maxThrusterAccelerationRate = 0.5f;
    private readonly float timeToMaxThrustAccel = 2f;
    private readonly float thrusterDecelerationRate = 0.5f;

    private float horizontalThrusterActiveTime = 0f;
    private float verticalThrusterActiveTime = 0f;
    private Vector3 currentVelocity;

    private void UpdateBridgeTransform() {
        bridge.transform.position = transform.position;
    }
    private void UpdateShipTransform()
    {
        float dt = Time.deltaTime;
        Vector3 forward = transform.forward;
        Vector3 horizontal = transform.right;
        Vector3 vertical = transform.up;

        UpdateThrusterActiveTime(dt);

        Vector3 impulseVelocity = CalculateAxisVelocity(forward, currentImpulse * maxImpulseSpeed,
                impulseAccelerationRate, impulseDecelerationRate, dt);

        Vector3 horizontalVelocity = CalculateAxisVelocity(horizontal, horizontalThrust * maxThrusterSpeed,
                GetThrusterAccelerationRate(horizontalThrusterActiveTime), thrusterDecelerationRate, dt);

        Vector3 verticalVelocity = CalculateAxisVelocity(vertical, verticalThrust * maxThrusterSpeed,
                GetThrusterAccelerationRate(verticalThrusterActiveTime), thrusterDecelerationRate, dt);

        currentVelocity = impulseVelocity + horizontalVelocity + verticalVelocity;

        HandleRotation(forward, dt);
        transform.position += currentVelocity * dt;
    }

    private void UpdateThrusterActiveTime(float dt)
    {
        if (Mathf.Abs(horizontalThrust) > 0.1f)
        {
            horizontalThrusterActiveTime += dt;
        }
        else
        {
            horizontalThrusterActiveTime = 0f;
        }

        if (Mathf.Abs(verticalThrust) > 0.1f)
        {
            verticalThrusterActiveTime += dt;
        }
        else
        {
            verticalThrusterActiveTime = 0f;
        }
    }
    
    private float GetThrusterAccelerationRate(float activeTime)
    {
        return Mathf.Lerp(baseThrusterAccelerationRate, maxThrusterAccelerationRate,
            Mathf.Clamp01(activeTime / timeToMaxThrustAccel));
    }

    private Vector3 CalculateAxisVelocity(Vector3 axis, float targetSpeed, float accelerationRate, float decelerationRate, float dt)
    {
        float currentSpeed = Vector3.Dot(currentVelocity, axis);
        float absoluteTarget = Mathf.Abs(targetSpeed);

        float rate;
        if (Mathf.Abs(currentSpeed) < absoluteTarget)
        {
            rate = accelerationRate;
        }
        else
        {
            rate = decelerationRate;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * dt);

        return axis * currentSpeed;
    }

    private void HandleRotation(Vector3 forwardDirection, float dt) 
    {
        float currentForwardSpeed = Vector3.Dot(currentVelocity, forwardDirection);

        if (Mathf.Abs(currentForwardSpeed) < 0.01f) {
            return;
        }

        Quaternion desiredRotation = Quaternion.Euler(0, currentHeading, 0);

        float forwardSpeedFactor = Mathf.Clamp01(currentForwardSpeed / maxImpulseSpeed);
        float rotationSpeed = rotationPower * forwardSpeedFactor;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, rotationSpeed * dt);
    }

}