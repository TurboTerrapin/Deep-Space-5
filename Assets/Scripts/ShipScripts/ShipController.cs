using UnityEngine;
public class ShipController : MonoBehaviour
{
    // Object References
    private GameObject controlHandler;

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
            GetPilotInput();
            GetTacticianInput();
            GetEngineerInput();
            GetCaptainInput();

            UpdateShipTransform();
        }
    }

    // *********** To Separate into own script in future ****************8

    private readonly float maxThrusterSpeed = 15f;
    private readonly float maxImpulseSpeed = 100f;
    private readonly float rotationPower = 35f;

    private readonly float impulseAccelerationRate = 4f;
    private readonly float impulseDecelerationRate = 10f;

    private readonly float baseThrusterAccelerationRate = 1f;
    private readonly float maxThrusterAccelerationRate = 5f;
    private readonly float timeToMaxThrustAccel = 2f;
    private readonly float thrusterDecelerationRate = 3.5f;

    private float horizontalThrusterActiveTime = 0f;
    private float verticalThrusterActiveTime = 0f;
    public Vector3 currentVelocity;

    private void UpdateShipTransform()
    {
        Vector3 forward = transform.forward;
        Vector3 horizontal = transform.right;
        Vector3 vertical = transform.up;

        UpdateThrusterActiveTimes();

        currentVelocity =
            CalculateAxisVelocity(forward, currentImpulse * maxImpulseSpeed, impulseAccelerationRate, impulseDecelerationRate) +
            CalculateHorizontalThrusterVelocity(horizontal) +
            CalculateVerticalThrusterVelocity(vertical);

        HandleRotation(forward);

        transform.position += currentVelocity * Time.deltaTime;
    }

    private void UpdateThrusterActiveTimes()
    {
        if (Mathf.Abs(horizontalThrust) > 0.1f)
        {
            horizontalThrusterActiveTime += Time.deltaTime;
        }
        else
        {
            horizontalThrusterActiveTime = 0f;
        }

        if (Mathf.Abs(verticalThrust) > 0.1f)
        {
            verticalThrusterActiveTime += Time.deltaTime;
        }
        else
        {
            verticalThrusterActiveTime = 0f;
        }
    }

    private Vector3 CalculateHorizontalThrusterVelocity(Vector3 axis)
    {
        float accelerationRate = Mathf.Lerp(
            baseThrusterAccelerationRate,
            maxThrusterAccelerationRate,
            Mathf.Clamp01(horizontalThrusterActiveTime / timeToMaxThrustAccel) 
        );

        return CalculateAxisVelocity(
            axis,
            horizontalThrust * maxThrusterSpeed,
            accelerationRate,
            thrusterDecelerationRate
        );
    }

    private Vector3 CalculateVerticalThrusterVelocity(Vector3 axis)
    {
        float accelerationRate = Mathf.Lerp(
            baseThrusterAccelerationRate,
            maxThrusterAccelerationRate,
            Mathf.Clamp01(verticalThrusterActiveTime / timeToMaxThrustAccel) 
        );

        return CalculateAxisVelocity(
            axis,
            verticalThrust * maxThrusterSpeed,
            accelerationRate,
            thrusterDecelerationRate
        );
    }

    private Vector3 CalculateAxisVelocity(Vector3 axis, float targetSpeed, float accelerationRate, float decelerationRate)
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

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        return axis * currentSpeed;
    }

    private void HandleRotation(Vector3 forwardDirection)
    {
        float currentForwardSpeed = Vector3.Dot(currentVelocity, forwardDirection);
        if (Mathf.Abs(currentForwardSpeed) > 0.01f)
        {
            float currentHeadingAngle = transform.eulerAngles.y;
            float headingDifference = Mathf.DeltaAngle(currentHeadingAngle, currentHeading);

            float rotationSpeed = Mathf.Clamp01(Mathf.Abs(currentForwardSpeed) / maxImpulseSpeed) * rotationPower;
            float rotationStep = rotationSpeed * Time.deltaTime;
            float newHeading = Mathf.MoveTowardsAngle(currentHeadingAngle, currentHeading, rotationStep * Mathf.Abs(headingDifference));

            transform.rotation = Quaternion.Euler(0, newHeading, 0);
        }
    }
}