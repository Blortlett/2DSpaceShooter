using System;
using Cinemachine;
using UnityEngine;

public class cPlayerController : MonoBehaviour
{
    private cCharacterController mCharacterController;

    // Player Inputs
    private PlayerInput mPlayerControls;
    private Vector2 mMoveInput;
    private Vector2 mMousePosition;
    

    // PlayerCamera reference
    [SerializeField] CinemachineVirtualCamera mPlayerCamera;

    // Set up input on awake
    private void Awake()
    {
        // Get controller component
        mPlayerControls = new PlayerInput();

        // Subscribe vars to input Events
        mPlayerControls.Player.Move.performed += ctx => mMoveInput = ctx.ReadValue<Vector2>();
        mPlayerControls.Player.Move.canceled += ctx => mMoveInput = Vector2.zero;
        mPlayerControls.Player.Look.performed += ctx => mMousePosition = ctx.ReadValue<Vector2>();
        
        // Enable input capture
        mPlayerControls.Player.Enable();
    }

    private void Start()
    {
        // Get attatched character controller
        mCharacterController = GetComponent<cCharacterController>();
        mCharacterController.SetCharacterType(cCharacterController.CharacterType.Player);

        // Subscribe to charController events
        mCharacterController.OnStartDrivingShip += CharacterController_OnStartDrivingShip;
        mCharacterController.OnStopDrivingShip += CharacterController_OnStopDrivingShip;

        // Input variables that toggle CharController Values directly
        mPlayerControls.Player.Interact.performed += ctx => mCharacterController.mIsInteracting = true;
        mPlayerControls.Player.Interact.canceled += ctx => mCharacterController.mIsInteracting = false;
        mPlayerControls.Player.Fire.performed += ctx => mCharacterController.PullWeaponTrigger();
        mPlayerControls.Player.Fire.canceled += ctx => mCharacterController.ReleaseWeaponTrigger();
    }

    private void UpdateCamera()
    {
        // Get the ship's Z rotation in radians
        float zRotation = mCharacterController.GetBoardedShip().GetZRotation();

        // Convert to a quaternion (rotate around Z-axis)
        mPlayerCamera.m_Lens.Dutch = zRotation;
    }

    private void Update()
    {
        UpdateCamera();
        MoveInput();
    }

    void MoveInput()
    {
        // Send input to the character controller
        mCharacterController.MoveInput(mMoveInput);
        mCharacterController.RotateCharacter(mMousePosition);
    }

    private void CharacterController_OnStopDrivingShip(object sender, EventArgs e)
    {
        Debug.Log("Player Stopped driving ship... player controls enabled");
        mPlayerControls.Enable();
    }

    private void CharacterController_OnStartDrivingShip(object sender, EventArgs e)
    {
        Debug.Log("Player started driving ship... player controls disabled");
        mPlayerControls.Disable();
    }
}
