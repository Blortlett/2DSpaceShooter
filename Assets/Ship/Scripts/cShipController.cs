using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class cShipController : MonoBehaviour, ISpaceship
{
    [Header("Ship Properties")]
    [SerializeField] private float shipAcceleration = 10f;
    [SerializeField] private float shipAngularAcceleration = 3f;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera shipCamera;

    [Header("Starting Crew")]
    [SerializeField] private cCharacterController[] charactersToStartBoarded;

    // Properties for state system access
    public float ShipAcceleration => shipAcceleration;
    public float ShipAngularAcceleration => shipAngularAcceleration;
    public Rigidbody2D Rigidbody { get; private set; }

    // Movement state system
    private ShipMovementStateMachine movementStateMachine;
    private PlayerDriveState playerDriveState;
    private MoveTowardsTargetState moveTowardsTargetState;
    private MatchSpeedState matchSpeedState;
    private FullThrottleState fullThrottleState;

    // Passenger management
    private List<IPassenger> charactersOnboard = new List<IPassenger>();

    // Player input
    private PlayerInput playerControls;
    private Vector2 shipMoveInput;
    private bool driverIsExiting;
    private bool isInputGoToTarget;
    private IPassenger currentDriver;

    // Boarding system
    private Vector2 boardingHatchLocalPosition;
    private List<cBoardingHatch> boardingHatchList = new List<cBoardingHatch>();

    // Combat
    [SerializeField] private cShipController combatTarget = null;

    // Graphics controller
    private scrShipGraphicManager shipGraphicManager;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeInput();
        Rigidbody = GetComponent<Rigidbody2D>();

        // Initialize state system
        movementStateMachine = new ShipMovementStateMachine(this);
        playerDriveState = new PlayerDriveState();
        moveTowardsTargetState = new MoveTowardsTargetState();
        matchSpeedState = new MatchSpeedState();
        fullThrottleState = new FullThrottleState();
    }

    private void Start()
    {
        // Get graphics controller from gameObject
        shipGraphicManager = GetComponent<scrShipGraphicManager>();

        // Set initial velocity
        Rigidbody.velocity = new Vector2(5, 0);

        // Add starting characters
        foreach (cCharacterController character in charactersToStartBoarded)
        {
            AddToCharactersOnBoard(character);
        }
    }

    private void Update()
    {
        HandleDriverExit();
    }

    private void FixedUpdate()
    {
        // Update movement state machine
        movementStateMachine.Update();
    }

    #endregion

    #region Input Handling

    private void InitializeInput()
    {
        playerControls = new PlayerInput();
        // wasd / move 2d input
        playerControls.DriveShip.Move.performed += ctx => OnMoveInputChanged(ctx.ReadValue<Vector2>());
        playerControls.DriveShip.Move.canceled += ctx => OnMoveInputChanged(Vector2.zero);
        // Stop driving button input
        playerControls.DriveShip.StopDriving.performed += ctx => driverIsExiting = true;
        playerControls.DriveShip.StopDriving.canceled += ctx => driverIsExiting = false;
        playerControls.DriveShip.DebugAbility1.performed += ctx => ToggleAutopilotToTarget();
    }

    private void OnMoveInputChanged(Vector2 input)
    {
        shipMoveInput = input;

        // Update player drive state if it's current
        if (movementStateMachine.GetCurrentState() is PlayerDriveState playerState)
        {
            playerState.SetMoveInput(shipMoveInput);
        }
    }

    private void HandleDriverExit()
    {
        if (driverIsExiting && currentDriver != null)
        {
            RemoveDriver();
        }
    }

    private void ToggleAutopilotToTarget()
    {
        moveTowardsTargetState.SetTarget(combatTarget.GetBoardingHatchTransform(), boardingHatchList[0].transform, 500f);
        movementStateMachine.ChangeState(moveTowardsTargetState);
    }

    #endregion

    #region Passenger Management

    public void AddToCharactersOnBoard(IPassenger character)
    {
        if (character == null)
        {
            Debug.LogWarning("Attempted to add a null character to the ship.");
            return;
        }

        charactersOnboard.Add(character);
        character.BoardShip(this);
        Debug.Log($"Added {character.GetCharacterType()} to the ship. Total onboard: {charactersOnboard.Count}");

        // if player onboard, ship should swap to internal graphics
        if (character.GetCharacterType() == cCharacterController.CharacterType.Player)
        {
            shipGraphicManager.ToggleInternalGraphics();
        }
    }

    public void PlayerDisembark(IPassenger character)
    {
        if (character == null || !charactersOnboard.Contains(character))
        {
            Debug.LogWarning("Character not found on the ship or is null.");
            return;
        }

        charactersOnboard.Remove(character);
        character.DisembarkShip();
        Debug.Log($"Removed {character.GetCharacterType()} from the ship. Total onboard: {charactersOnboard.Count}");
    }

    public List<IPassenger> GetCharactersOnboard() => charactersOnboard;

    #endregion

    #region Driving System

    public void PassengerDriveShip(IPassenger character)
    {
        if (character == null) return;

        currentDriver = character;
        playerControls.Enable();
        character.OnPossessShip(this);

        // Switch to player drive state
        playerDriveState.SetMoveInput(shipMoveInput);
        movementStateMachine.ChangeState(playerDriveState);

        // Swap to drive ship camera
        shipCamera.Priority = 11;

        // Toggle external ship graphics
        shipGraphicManager.ToggleExternalGraphics();
    }

    public void RemoveDriver()
    {
        if (currentDriver == null) return;

        playerControls.Disable();
        currentDriver.StopDrivingShip();
        currentDriver = null;

        // Switch to idle state
        movementStateMachine.ChangeState(new IdleState());

        // Swap to player focused camera
        shipCamera.Priority = 9;
        driverIsExiting = false;

        // Toggle internal ship graphics
        shipGraphicManager.ToggleInternalGraphics();
    }

    #endregion

    #region Movement State Control (Public API)

    public void SetMoveTowardsTarget(Transform target, float speed = 5f)
    {
        var moveState = new MoveTowardsTargetState();
        moveState.SetTarget(target, GetBoardingHatchTransform(), speed);
        movementStateMachine.ChangeState(moveState);
    }

    public void SetMatchSpeed(Rigidbody2D target, float force = 10f)
    {
        var matchState = new MatchSpeedState();
        matchState.SetTarget(target, force);
        movementStateMachine.ChangeState(matchState);
    }

    public void SetFullThrottle(Vector2 direction = default)
    {
        var throttleState = new FullThrottleState();
        if (direction != Vector2.zero)
        {
            throttleState.SetDirection(direction);
        }
        movementStateMachine.ChangeState(throttleState);
    }

    public void SetIdle()
    {
        movementStateMachine.ChangeState(new IdleState());
    }

    #endregion

    #region Boarding System

    public void AddBoardingHatch(cBoardingHatch boardingHatch)
    {
        if (boardingHatch == null) return;

        boardingHatchList.Add(boardingHatch);
        boardingHatchLocalPosition = boardingHatch.transform.position - transform.position;
        Debug.Log("Added hatch to ship");
    }

    public Transform GetBoardingHatchTransform()
    {
        // dodgy
        return boardingHatchList[0].transform;
    }

    #endregion

    #region ISpaceship Implementation

    public Vector2 GetPosition() => transform.position;
    public GameObject GetShip() => gameObject;
    public string GetShipName() => gameObject.name;
    public float GetZRotation() => transform.eulerAngles.z;

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        playerControls?.Dispose();
    }

    #endregion
}