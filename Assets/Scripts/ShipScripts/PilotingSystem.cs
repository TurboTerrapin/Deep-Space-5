using UnityEngine;
using UnityEngine;

public class PilotingSystem : MonoBehaviour
{
    [Header("Control References")]
    private GameObject controlHandler;

    [Header("Speed Settings")]
    private float maxThrusterSpeed = 8f;
    private float maxImpulseSpeed = 30f;

    [Header("Rotation Settings")]
     private float rotationPower = 6f;
     private float steeringResponsiveness = 5f;
     private float maxRotationSpeed = 25f;

    [Header("Impulse Settings")]
    private float impulseAccelerationRate = 0.8f;
    private float impulseDecelerationRate = 1.75f;

    [Header("Thruster Settings")]
    private float baseThrusterAccelerationRate = 0.5f;
    private float maxThrusterAccelerationRate = 1.5f;
    private float timeToMaxThrustAccel = 2f;
    private float thrusterDecelerationRate = 0.5f;

    // Component references
    private ImpulseThrottle impulseThrottle;
    private CourseHeading courseHeading;
    private HorizontalThrusters horizontalThrusters;
    private VerticalThrusters verticalThrusters;

    // Input values
    private float currentImpulse;
    private float steeringInput;
    private float horizontalThrust;
    private float verticalThrust;

    // Movement state
    private float smoothedSteeringInput = 0f;
    private float horizontalThrusterActiveTime;
    private float verticalThrusterActiveTime;
    public float currentRotationSpeed;
    public float forwardSpeed;
    public Vector3 currentVelocity;

    public float currentImpulseSpeed = 0f;
    public float currentHorizontalSpeed = 0f;
    public float currentVerticalSpeed = 0f;

    public bool AssignControlReferences(GameObject controlHandler)
    {
        impulseThrottle = controlHandler.GetComponent<ImpulseThrottle>();
        courseHeading = controlHandler.GetComponent<CourseHeading>();
        horizontalThrusters = controlHandler.GetComponent<HorizontalThrusters>();
        verticalThrusters = controlHandler.GetComponent<VerticalThrusters>();

        return impulseThrottle && courseHeading &&
               horizontalThrusters && verticalThrusters;
    }

    public void UpdateInput()
    {
        currentImpulse = impulseThrottle.getCurrentImpulse();
        steeringInput = courseHeading.getSteeringValue();
        horizontalThrust = horizontalThrusters.getHorizontalThrusterState();
        verticalThrust = verticalThrusters.getVerticalThrusterState();
    }

    public void UpdateMovement(Transform worldRoot)
    {
        float dt = Time.deltaTime;

        Vector3 forward = transform.forward;
        Vector3 horizontal = -transform.right;
        Vector3 vertical = transform.up;

        // Update impulse speed
        currentImpulseSpeed = Mathf.MoveTowards(
            currentImpulseSpeed,
            currentImpulse * maxImpulseSpeed,
            ((Mathf.Abs(currentImpulseSpeed) < Mathf.Abs(currentImpulse * maxImpulseSpeed)) ? impulseAccelerationRate : impulseDecelerationRate) * dt
        );

        // Update horizontal thruster speed
        float horizontalRate = GetThrusterAccelerationRate(horizontalThrusterActiveTime);
        currentHorizontalSpeed = Mathf.MoveTowards(
            currentHorizontalSpeed,
            horizontalThrust * maxThrusterSpeed,
            ((Mathf.Abs(currentHorizontalSpeed) < Mathf.Abs(horizontalThrust * maxThrusterSpeed)) ? horizontalRate : thrusterDecelerationRate) * dt
        );

        // Update vertical thruster speed
        float verticalRate = GetThrusterAccelerationRate(verticalThrusterActiveTime);
        currentVerticalSpeed = Mathf.MoveTowards(
            currentVerticalSpeed,
            verticalThrust * maxThrusterSpeed,
            ((Mathf.Abs(currentVerticalSpeed) < Mathf.Abs(verticalThrust * maxThrusterSpeed)) ? verticalRate : thrusterDecelerationRate) * dt
        );

        Vector3 impulseVelocity = forward * currentImpulseSpeed;
        Vector3 horizontalVelocity = horizontal * currentHorizontalSpeed;
        Vector3 verticalVelocity = vertical * currentVerticalSpeed;

        currentVelocity = impulseVelocity + horizontalVelocity + verticalVelocity;

        if (currentVelocity.magnitude > maxImpulseSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxImpulseSpeed;
        }

        if (worldRoot != null)
        {
            worldRoot.position -= currentVelocity * dt;
        }

        forwardSpeed = currentVelocity.magnitude;
        HandleRotation(dt);
    }

    private float GetThrusterAccelerationRate(float activeTime)
    {
        return Mathf.Lerp(baseThrusterAccelerationRate, maxThrusterAccelerationRate,
            Mathf.Clamp01(activeTime / timeToMaxThrustAccel));
    }

    private void HandleRotation(float dt)
    {
        forwardSpeed = currentVelocity.magnitude;
        float speedFactor = Mathf.Clamp01(forwardSpeed / maxImpulseSpeed);

        smoothedSteeringInput = Mathf.Lerp(
            smoothedSteeringInput,
            steeringInput,
            steeringResponsiveness * dt
        );

        float targetRotationSpeed = smoothedSteeringInput * maxRotationSpeed * speedFactor;

        if (Mathf.Abs(forwardSpeed) < 0.01f)
            return;

        currentRotationSpeed = Mathf.Lerp(
            currentRotationSpeed,
            targetRotationSpeed,
            rotationPower * dt
        );

        if (Mathf.Abs(steeringInput) < 0.1f && Mathf.Abs(smoothedSteeringInput) < 0.1f)
        {
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, 0f, rotationPower * dt);
        }

        transform.Rotate(0f, currentRotationSpeed * dt, 0f);
    }
}