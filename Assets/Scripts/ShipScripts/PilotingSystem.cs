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
    private float currentHeading;
    private float horizontalThrust;
    private float verticalThrust;

    // Movement state
    private float horizontalThrusterActiveTime;
    private float verticalThrusterActiveTime;
    private Vector3 currentVelocity;

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
        currentHeading = courseHeading.getCurrentHeading();
        horizontalThrust = horizontalThrusters.getHorizontalThrusterState();
        verticalThrust = verticalThrusters.getVerticalThrusterState();
    }

    public void UpdateMovement()
    {
        float dt = Time.deltaTime;
        Vector3 forward = -transform.forward;
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
}