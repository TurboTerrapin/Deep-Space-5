/*
    Handles ship movement - Vertical & Horizontal Thrusters, Impulse Throttle, and Course Heading
*/

using UnityEngine;

public class PilotingSystem : MonoBehaviour
{
    [Header("Control References")]
    [SerializeField] private GameObject controlHandler;

    [Header("Speed Settings")]
    [SerializeField] private float maxThrusterSpeed = 5f;
    [SerializeField] private float maxImpulseSpeed = 40f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationPower = 6f;

    [Header("Impulse Settings")]
    [SerializeField] private float impulseAccelerationRate = 0.4f;
    [SerializeField] private float impulseDecelerationRate = 1.5f;

    [Header("Thruster Settings")]
    [SerializeField] private float baseThrusterAccelerationRate = 0.1f;
    [SerializeField] private float maxThrusterAccelerationRate = 0.5f;
    [SerializeField] private float timeToMaxThrustAccel = 2f;
    [SerializeField] private float thrusterDecelerationRate = 0.5f;

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
    private float horizontalThrusterActiveTime;
    private float verticalThrusterActiveTime;
    public Vector3 currentVelocity;

    public bool AssignControlReferences(GameObject controlHandler)
    {
        impulseThrottle = controlHandler.GetComponent<ImpulseThrottle>();
        courseHeading = controlHandler.GetComponent<CourseHeading>();
        horizontalThrusters = controlHandler.GetComponent<HorizontalThrusters>();
        verticalThrusters = controlHandler.GetComponent<VerticalThrusters>();

        return impulseThrottle  && courseHeading &&
               horizontalThrusters  && verticalThrusters;
    }

    public void UpdateInput()
    {
        currentImpulse = impulseThrottle.getCurrentImpulse();
        steeringInput = courseHeading.getSteeringValue();
        horizontalThrust = horizontalThrusters.getHorizontalThrusterState();
        verticalThrust = verticalThrusters.getVerticalThrusterState();
    }
    public float currentImpulseSpeed = 0f;
    public float currentHorizontalSpeed = 0f;
    public float currentVerticalSpeed = 0f;
    public void UpdateMovement()
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

        // Combine velocities
        Vector3 impulseVelocity = forward * currentImpulseSpeed;
        Vector3 horizontalVelocity = horizontal * currentHorizontalSpeed;
        Vector3 verticalVelocity = vertical * currentVerticalSpeed;

        currentVelocity = impulseVelocity + horizontalVelocity + verticalVelocity;

        if (currentVelocity.magnitude > maxImpulseSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxImpulseSpeed;
        }

        transform.position += currentVelocity * dt;

        // Handle rotation using the fixed forwardSpeed:
        forwardSpeed = currentVelocity.magnitude;
        HandleRotation(dt);
    }

    private void UpdateThrusterActiveTime(float dt)
    {
        horizontalThrusterActiveTime = Mathf.Abs(horizontalThrust) > 0.1f ?
            horizontalThrusterActiveTime + dt : 0f;

        verticalThrusterActiveTime = Mathf.Abs(verticalThrust) > 0.1f ?
            verticalThrusterActiveTime + dt : 0f;
    }

    private float GetThrusterAccelerationRate(float activeTime)
    {
        return Mathf.Lerp(baseThrusterAccelerationRate, maxThrusterAccelerationRate,
            Mathf.Clamp01(activeTime / timeToMaxThrustAccel));
    }

    private Vector3 CalculateAxisVelocity(Vector3 axis, float targetSpeed,
        float accelerationRate, float decelerationRate, float dt)
    {
        float currentSpeed = Vector3.Dot(currentVelocity, axis);
        float rate = Mathf.Abs(currentSpeed) < Mathf.Abs(targetSpeed) ?
            accelerationRate : decelerationRate;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * dt);
        return axis * currentSpeed;
    }

    /*
    private void HandleRotation(Vector3 forwardDirection, float dt)
    {
        float currentForwardSpeed = Vector3.Dot(currentVelocity, forwardDirection);
        if (Mathf.Abs(currentForwardSpeed) < 0.01f) return;

        Quaternion desiredRotation = Quaternion.Euler(0, currentHeading, 0);
        float forwardSpeedFactor = Mathf.Clamp01(currentForwardSpeed / maxImpulseSpeed);
        float rotationSpeed = rotationPower * forwardSpeedFactor;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            rotationSpeed * dt
        );
    }
    */

    private float smoothedSteeringInput = 0f; // otational inertia
    [SerializeField] private float steeringResponsiveness = 5f; // lower is slower


    [SerializeField] private float maxRotationSpeed = 25f; // degrees per second at full steering
    public float currentRotationSpeed; // Current rotation speed in degrees/sec
    public float forwardSpeed;
    private void HandleRotation(float dt)
    {
        forwardSpeed = currentVelocity.magnitude;
        float speedFactor = Mathf.Clamp01(forwardSpeed / maxImpulseSpeed);

        // Smooth the steering input to simulate rotational inertia
        smoothedSteeringInput = Mathf.Lerp(
            smoothedSteeringInput,
            steeringInput,
            steeringResponsiveness * dt
        );

        float targetRotationSpeed = smoothedSteeringInput * maxRotationSpeed * speedFactor;

        // If almost no forward movement, don't rotate
        if (Mathf.Abs(forwardSpeed) < 0.01f)
            return;

        // Adjust current rotation speed toward target
        currentRotationSpeed = Mathf.Lerp(
            currentRotationSpeed,
            targetRotationSpeed,
            rotationPower * dt
        );

        // Dampen rotation speed to zero when steering is near neutral
        if (Mathf.Abs(steeringInput) < 0.1f && Mathf.Abs(smoothedSteeringInput) < 0.1f)
        {
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, 0f, rotationPower * dt);
        }

        // Apply rotation
        transform.Rotate(0f, currentRotationSpeed * dt, 0f);
    }


}