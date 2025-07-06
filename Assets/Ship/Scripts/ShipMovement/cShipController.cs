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

    // -= Ship Managers =-
    // Local Projectile Manager for character bullets
    scrProjectileManager projectileManager;
    // Passenger management
    private cShipPassengerManager passengerManager;

    // Player input
    private PlayerInput playerControls;
    private Vector2 shipMoveInput;
    private bool driverIsExiting;

    // Boarding system
    private List<cBoardingHatch> boardingHatchList = new List<cBoardingHatch>();
    private Vector2 boardingHatchLocalPosition;

    // Combat
    [SerializeField] private cShipController combatTarget = null;

    // Graphics controller
    private scrShipGraphicManager shipGraphicManager;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeInput();
        Rigidbody = GetComponent<Rigidbody2D>();

        // Init projectileManager
        projectileManager = GetComponentInChildren<scrProjectileManager>();
        // Initialize passenger manager
        passengerManager = new cShipPassengerManager(this, projectileManager);
        SetupPassengerManagerEvents();

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

        // Add starting characters using passenger manager
        passengerManager.InitializeWithPassengers(charactersToStartBoarded);
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

    #region Passenger Manager Setup

    private void SetupPassengerManagerEvents()
    {
        passengerManager.OnPassengerBoarded += OnPassengerBoarded;
        passengerManager.OnPassengerDisembarked += OnPassengerDisembarked;
        passengerManager.OnDriverChanged += OnDriverChanged;
        passengerManager.OnDriverRemoved += OnDriverRemoved;
    }

    private void OnPassengerBoarded(IPassenger passenger)
    {
        // Handle graphics changes when player boards
        if (passenger.GetCharacterType() == cCharacterController.CharacterType.Player)
        {
            shipGraphicManager.ToggleInternalGraphics();
        }
    }

    private void OnPassengerDisembarked(IPassenger passenger)
    {
        // Handle any cleanup when passengers leave
        Debug.Log($"Passenger {passenger.GetCharacterType()} has left the ship");
    }

    private void OnDriverChanged(IPassenger newDriver)
    {
        playerControls.Enable();

        // Switch to player drive state
        playerDriveState.SetMoveInput(shipMoveInput);
        movementStateMachine.ChangeState(playerDriveState);

        // Swap to drive ship camera
        shipCamera.Priority = 11;

        // Toggle external ship graphics
        shipGraphicManager.ToggleExternalGraphics();
    }

    private void OnDriverRemoved()
    {
        playerControls.Disable();

        // Switch to idle state
        movementStateMachine.ChangeState(new IdleState());

        // Swap to player focused camera
        shipCamera.Priority = 9;
        driverIsExiting = false;

        // Toggle internal ship graphics
        shipGraphicManager.ToggleInternalGraphics();
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
        if (driverIsExiting && passengerManager.HasDriver)
        {
            passengerManager.RemoveDriver();
        }
    }

    private void ToggleAutopilotToTarget()
    {
        moveTowardsTargetState.SetTarget(combatTarget.GetBoardingHatchTransform(), boardingHatchList[0].transform, 500f);
        movementStateMachine.ChangeState(moveTowardsTargetState);
    }

    #endregion

    #region Public Passenger API (Delegates to PassengerManager)

    public void AddToCharactersOnBoard(IPassenger character)
    {
        passengerManager.AddPassenger(character);
    }

    public void PlayerDisembark(IPassenger character)
    {
        passengerManager.RemovePassenger(character);
        if (character.GetCharacterType() == cCharacterController.CharacterType.Player)
        {
            // Toggle external ship graphics
            shipGraphicManager.ToggleExternalGraphics();
        }
    }

    public void PassengerDriveShip(IPassenger character)
    {
        passengerManager.SetDriver(character);
    }

    public void RemoveDriver()
    {
        passengerManager.RemoveDriver();
    }

    public List<IPassenger> GetCharactersOnboard()
    {
        return passengerManager.CharactersOnboard;
    }

    // Additional convenience methods
    public bool HasPassengerOfType(cCharacterController.CharacterType characterType)
    {
        return passengerManager.HasPassengerOfType(characterType);
    }

    public IPassenger GetCurrentDriver()
    {
        return passengerManager.CurrentDriver;
    }

    public int GetPassengerCount()
    {
        return passengerManager.PassengerCount;
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
    public Rigidbody2D GetShipRigidbody() => Rigidbody;
    public Transform GetBoardingHatch() => boardingHatchList[0].transform;
    public float GetZRotation() => transform.eulerAngles.z;

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        playerControls?.Dispose();

        // Clean up passenger manager events
        if (passengerManager != null)
        {
            passengerManager.OnPassengerBoarded -= OnPassengerBoarded;
            passengerManager.OnPassengerDisembarked -= OnPassengerDisembarked;
            passengerManager.OnDriverChanged -= OnDriverChanged;
            passengerManager.OnDriverRemoved -= OnDriverRemoved;
        }
    }

    #endregion
}