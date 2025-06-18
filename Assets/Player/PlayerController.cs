using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPassenger
{
    // Constant values
    [SerializeField] float PLAYER_ACCELERATION = 5f;
    [SerializeField] float PLAYER_MAXSPEED = 1f;
    [SerializeField] float PLAYER_DRAG = 0.9f;

    // PlayerCamera reference
    [SerializeField] CinemachineVirtualCamera mPlayerCamera;

    // Inputs
    private PlayerInput mPlayerControls;
    private Vector2 mMoveInput;
    private Vector2 mMousePosition;
    private bool mIsInteracting;

    // Player physics
    private Vector2 mVelocity;

    // Current Ship Data
    bool IsAboardShip;
    bool mIsDrivingShip;
    private ISpaceship mCurrentBoardedSpaceship;
    private Vector2 mPositionInsideShip;

    // World Position
    private Vector2 mWorldPostion;

    // Latest interactable nearby player
    private List<IInteractable> mInteractablesNearby = new List<IInteractable>();

    private void Awake()
    {
        // Get controller component
        mPlayerControls = new PlayerInput();

        // Subscribe vars to input Events
        mPlayerControls.Player.Move.performed += ctx => mMoveInput = ctx.ReadValue<Vector2>();
        mPlayerControls.Player.Move.canceled += ctx => mMoveInput = Vector2.zero;
        mPlayerControls.Player.Look.performed += ctx => mMousePosition = ctx.ReadValue<Vector2>();
        mPlayerControls.Player.Interact.performed += ctx => mIsInteracting = true;

        mPlayerControls.Player.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Input n that
    private void Update()
    {
        RotateCharacter();
        MoveInput();
        HandleInteract();
        UpdateCamera();
        UpdatePlayerPosition();
    }

    private void UpdateCamera()
    {
        // Get the ship's Z rotation in radians
        float zRotation = mCurrentBoardedSpaceship.GetZRotation();

        // Convert to a quaternion (rotate around Z-axis)
        mPlayerCamera.m_Lens.Dutch = zRotation;
    }

    // Physics stuff
    void FixedUpdate()
    {
        MoveInsideShip();
    }

    void MoveInput()
    {
        // Apply acceleration
        Vector2 AddVelocity = mMoveInput * PLAYER_ACCELERATION;
        mVelocity += AddVelocity * Time.deltaTime;

        // Apply drag
        mVelocity *= Mathf.Pow(PLAYER_DRAG, Time.deltaTime);

        // Clamp max velocity
        if (mVelocity.magnitude > PLAYER_MAXSPEED)
        {
            mVelocity = mVelocity.normalized * PLAYER_MAXSPEED;
        }
    }

    void MoveInsideShip()
    {
        // Get the ship's Z rotation in radians
        float zRotation = Mathf.Deg2Rad * mCurrentBoardedSpaceship.GetZRotation();

        // Rotate the character's velocity to align with the ship's orientation
        Vector2 rotatedVelocity;
        rotatedVelocity.x = mVelocity.x * Mathf.Cos(zRotation) - mVelocity.y * Mathf.Sin(zRotation);
        rotatedVelocity.y = mVelocity.x * Mathf.Sin(zRotation) + mVelocity.y * Mathf.Cos(zRotation);

        // Apply rotated velocity to character position inside ship
        mPositionInsideShip += rotatedVelocity * Time.fixedDeltaTime;
    }

    void UpdatePlayerPosition()
    {
        // Get the ship's Z rotation in radians
        float zRotation = Mathf.Deg2Rad * mCurrentBoardedSpaceship.GetZRotation();

        // Rotate mPositionInsideShip by ship's zRotation
        Vector2 RealShipPosition;
        RealShipPosition.x = mPositionInsideShip.x * Mathf.Cos(zRotation) - mPositionInsideShip.y * Mathf.Sin(zRotation);
        RealShipPosition.y = mPositionInsideShip.x * Mathf.Sin(zRotation) + mPositionInsideShip.y * Mathf.Cos(zRotation);

        // Calculate world position
        mWorldPostion = mCurrentBoardedSpaceship.GetPosition() + RealShipPosition;

        // Set game object to world position
        transform.position = mWorldPostion;
    }

    void RotateCharacter()
    {
        // Convert mouse screen position to world position in 2D
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mMousePosition);

        // Calculate direction from player to mouse
        Vector2 direction = mouseWorldPos - (Vector2)transform.position;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation around Z-axis (for 2D top-down)
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleInteract()
    {
        // Only when player is interacting
        if (!mIsInteracting) return;

        // only if there are items nearby
        if (mInteractablesNearby.Count > 0)
        {   // interact with first item on list
            mInteractablesNearby[0].OnInteract(this);
        }
    }

    public void BoardShip(ISpaceship _Ship)
    {
        mCurrentBoardedSpaceship = _Ship;
        IsAboardShip = true;
    }

    public void DisembarkShip()
    {
        mCurrentBoardedSpaceship = null;
        IsAboardShip = false;
    }

    public ISpaceship GetBoardedShip()
    {
        return mCurrentBoardedSpaceship;
    }

    public string GetName()
    {
        return gameObject.name;
    }

    public void OnPossessShip(ISpaceship _Ship)
    {
        mIsDrivingShip = true;
        mPlayerControls.Disable();
    }

    public void StopDrivingShip()
    {
        mIsInteracting = false;
        mIsDrivingShip = false;
        mPlayerControls.Enable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            mInteractablesNearby.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            mInteractablesNearby.Remove(interactable);
        }
    }
}
