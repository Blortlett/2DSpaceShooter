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
    private Vector2 moveInput;

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
        // Apply thrust based on player input
        Vector2 thrustForce = new Vector2(moveInput.y * ship.ShipAcceleration, 0);
        ship.Rigidbody.AddRelativeForce(thrustForce);

        // Apply rotation based on player input
        float torqueForce = (moveInput.x * -1) * ship.ShipAngularAcceleration;
        ship.Rigidbody.AddTorque(torqueForce);
    }

    public void Exit(cShipController ship)
    {
        Debug.Log("Exiting Player Drive State");
    }
}

// State for moving towards a specific target
public class MoveTowardsTargetState : IShipMovementState
{
    private Transform target;
    private Transform localShipHatchTransform;
    private float approachSpeed = 5f;
    private float rotationSpeed = 2f;

    public void SetTarget(Transform targetTransform, Transform _localShipHatchTransform, float speed = 5f)
    {
        target = targetTransform;
        approachSpeed = speed;
        localShipHatchTransform = _localShipHatchTransform;
    }

    public void Enter(cShipController ship)
    {
        Debug.Log("Entering Move Towards Target State");
    }

    public void Execute(cShipController ship)
    {
        if (target == null) return;

        Vector2 directionToTarget = (target.position - localShipHatchTransform.position).normalized;

        // Calculate desired rotation
        float targetAngle = target.transform.eulerAngles.z;
        float angleDifference = Mathf.DeltaAngle(ship.transform.eulerAngles.z, targetAngle);

        // Apply rotation towards target
        float torque = Mathf.Clamp(angleDifference * rotationSpeed, -ship.ShipAngularAcceleration, ship.ShipAngularAcceleration);
        ship.Rigidbody.AddTorque(torque);

        // Apply thrust towards target
        Vector2 thrustForce = directionToTarget * approachSpeed;
        ship.Rigidbody.AddForce(thrustForce);
    }

    public void Exit(cShipController ship)
    {
        Debug.Log("Exiting Move Towards Target State");
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
        Debug.Log("Entering Idle State");
    }

    public void Execute(cShipController ship)
    {
        // Apply light drag or do nothing
        // Could add station-keeping logic here
    }

    public void Exit(cShipController ship)
    {
        Debug.Log("Exiting Idle State");
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