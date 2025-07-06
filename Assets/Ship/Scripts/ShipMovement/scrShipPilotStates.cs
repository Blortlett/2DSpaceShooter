// Base interface for ship movement states
using UnityEngine;

public interface IShipMovementState
{
    void Enter(cShipController ship);
    void Execute(cShipController ship);
    void Exit(cShipController ship);
}

// State for player-controlled driving
public class PlayerDriveState : IShipMovementState
{
    // Player INPUT variable
    private Vector2 moveInput;

    // Constants
    // Drag
    private float HORIZONTAL_DRAG = .8f;
    // Max Speed
    private float MAX_SPEED = 12f;

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void Enter(cShipController ship)
    {
        Debug.Log("Entering Player Drive State");
    }

    public void Execute(cShipController ship)
    {
        // -= Apply thrust based on player input =-
        Vector2 thrustForce = new Vector2(moveInput.y * ship.ShipAcceleration, 0);
        ship.Rigidbody.AddRelativeForce(thrustForce);

        // Apply drag
        Vector3 localVelocity = ship.transform.InverseTransformDirection(ship.Rigidbody.velocity);
        localVelocity.y *= HORIZONTAL_DRAG; // Apply drag to local Y
        ship.Rigidbody.velocity = ship.transform.TransformDirection(localVelocity);

        // Clamp max velocity
        if (ship.Rigidbody.velocity.magnitude > MAX_SPEED)
        {
            ship.Rigidbody.velocity = ship.Rigidbody.velocity.normalized * MAX_SPEED;
        }

        // -= Apply rotation based on player input =-
        float torqueForce = (moveInput.x * -1) * ship.ShipAngularAcceleration;
        ship.Rigidbody.AddTorque(torqueForce);
    }

    public void Exit(cShipController ship)
    {
        //Debug.Log("Exiting Player Drive State");
    }
}

// State for moving towards a specific target with velocity matching
public class MoveTowardsTargetState : IShipMovementState
{
    private Transform target;
    private Transform localShipHatchTransform;
    private Rigidbody2D targetRigidbody;
    private float maxApproachSpeed = 5f;
    private float rotationSpeed = 2f;
    private float decelerationDistance = 20f;
    private float arrivalRadius = 2f;
    private float velocityMatchingStrength = 5f;

    public void SetTarget(Transform targetTransform, Transform localHatchTransform, float speed = 5f, float decelDistance = 20f, float arrivalDist = 2f)
    {
        target = targetTransform;
        localShipHatchTransform = localHatchTransform;
        maxApproachSpeed = speed;
        decelerationDistance = decelDistance;
        arrivalRadius = arrivalDist;

        // Try to get target's rigidbody for velocity matching
        targetRigidbody = target.GetComponent<Rigidbody2D>();
    }

    public void Enter(cShipController ship)
    {
        Debug.Log("Entering Move Towards Target State");
    }

    public void Execute(cShipController ship)
    {
        if (target == null || localShipHatchTransform == null) return;

        Vector2 shipPosition = localShipHatchTransform.position;
        Vector2 targetPosition = target.position;
        Vector2 toTarget = targetPosition - shipPosition;
        float distanceToTarget = toTarget.magnitude;
        Vector2 directionToTarget = toTarget.normalized;

        // Calculate desired velocity based on distance
        Vector2 desiredVelocity = CalculateDesiredVelocity(ship, toTarget, distanceToTarget, directionToTarget);

        // Calculate force needed to achieve desired velocity
        Vector2 velocityDifference = desiredVelocity - ship.Rigidbody.velocity;
        Vector2 thrustForce = velocityDifference * ship.ShipAcceleration;

        // Apply the thrust force
        ship.Rigidbody.AddForce(thrustForce);

        // Handle rotation to match target's orientation
        HandleRotation(ship);
    }

    private Vector2 CalculateDesiredVelocity(cShipController ship, Vector2 toTarget, float distance, Vector2 direction)
    {
        // Taking Target's velocity into account
        Vector2 targetVelocity = Vector2.zero;
        if (targetRigidbody != null)
        {
            Debug.Log("Approaching moving ship");
            targetVelocity = targetRigidbody.velocity;
            // Calculate max approach speed (Slightly higher than target velocity magnitude)
            maxApproachSpeed = targetVelocity.magnitude;
            maxApproachSpeed *= 1.25f;
        }
        

        // If we're very close, just match target velocity
        if (distance <= arrivalRadius)
        {
            return targetVelocity;
        }

        // Calculate approach velocity based on distance
        float approachSpeed;
        if (distance <= decelerationDistance)
        {
            // Deceleration zone - slow down as we get closer
            float decelerationFactor = distance / decelerationDistance;
            approachSpeed = Mathf.Lerp(0f, maxApproachSpeed, decelerationFactor);

            // Also consider how fast we're already moving towards the target
            Vector2 currentVelocityTowardsTarget = Vector3.Project(ship.Rigidbody.velocity, direction);
            float currentSpeed = currentVelocityTowardsTarget.magnitude;

            // If we're moving too fast towards target, reduce desired speed further
            if (Vector2.Dot(currentVelocityTowardsTarget, direction) > 0 && currentSpeed > approachSpeed)
            {
                approachSpeed = Mathf.Max(0f, approachSpeed - (currentSpeed - approachSpeed) * 0.5f);
            }
        }
        else
        {
            // Full speed approach
            approachSpeed = maxApproachSpeed;
        }

        // Desired velocity is approach velocity plus target velocity
        Vector2 approachVelocity = direction * approachSpeed;
        Vector2 desiredVelocity = approachVelocity;// + targetVelocity;

        return desiredVelocity;
    }

    private void HandleRotation(cShipController ship)
    {
        // Match target's rotation
        float targetAngle = target.transform.eulerAngles.z;
        float angleDifference = Mathf.DeltaAngle(ship.transform.eulerAngles.z, targetAngle);

        // Apply rotation towards target orientation
        float torque = Mathf.Clamp(angleDifference * rotationSpeed, -ship.ShipAngularAcceleration, ship.ShipAngularAcceleration);
        ship.Rigidbody.AddTorque(torque);
    }

    public void Exit(cShipController ship)
    {
        //Debug.Log("Exiting Move Towards Target State");
    }

    // Helper method to check if we've successfully reached the target
    public bool HasReachedTarget()
    {
        if (target == null || localShipHatchTransform == null) return false;

        float distance = Vector2.Distance(localShipHatchTransform.position, target.position);
        return distance <= arrivalRadius;
    }
}

// State for matching speed with another object
public class MatchSpeedState : IShipMovementState
{
    private Rigidbody2D targetRigidbody;
    private float matchingForce = 10f;

    public void SetTarget(Rigidbody2D target, float force = 10f)
    {
        targetRigidbody = target;
        matchingForce = force;
    }

    public void Enter(cShipController ship)
    {
        Debug.Log("Entering Match Speed State");
    }

    public void Execute(cShipController ship)
    {
        if (targetRigidbody == null) return;

        Vector2 velocityDifference = targetRigidbody.velocity - ship.Rigidbody.velocity;
        Vector2 forceToApply = velocityDifference * matchingForce;

        ship.Rigidbody.AddForce(forceToApply);
    }

    public void Exit(cShipController ship)
    {
        Debug.Log("Exiting Match Speed State");
    }
}

// State for full throttle in current direction
public class FullThrottleState : IShipMovementState
{
    private Vector2 thrustDirection;

    public void SetDirection(Vector2 direction)
    {
        thrustDirection = direction.normalized;
    }

    public void Enter(cShipController ship)
    {
        Debug.Log("Entering Full Throttle State");
        // Use ship's current forward direction if no direction specified
        if (thrustDirection == Vector2.zero)
        {
            thrustDirection = ship.transform.up; // Assuming ship faces "up" in local space
        }
    }

    public void Execute(cShipController ship)
    {
        Vector2 thrustForce = thrustDirection * ship.ShipAcceleration;
        ship.Rigidbody.AddForce(thrustForce);
    }

    public void Exit(cShipController ship)
    {
        Debug.Log("Exiting Full Throttle State");
    }
}

// State for idle/stationary
public class IdleState : IShipMovementState
{
    public void Enter(cShipController ship)
    {
        //Debug.Log("Entering Idle State");
    }

    public void Execute(cShipController ship)
    {
        // Apply light drag or do nothing
        // Could add station-keeping logic here
    }

    public void Exit(cShipController ship)
    {
        //Debug.Log("Exiting Idle State");
    }
}

// Movement state manager
public class ShipMovementStateMachine
{
    private IShipMovementState currentState;
    private cShipController ship;

    public ShipMovementStateMachine(cShipController shipController)
    {
        ship = shipController;
        currentState = new IdleState();
        currentState.Enter(ship);
    }

    public void ChangeState(IShipMovementState newState)
    {
        currentState?.Exit(ship);
        currentState = newState;
        currentState?.Enter(ship);
    }

    public void Update()
    {
        currentState?.Execute(ship);
    }

    public IShipMovementState GetCurrentState()
    {
        return currentState;
    }

    public T GetCurrentStateAs<T>() where T : class, IShipMovementState
    {
        return currentState as T;
    }
}