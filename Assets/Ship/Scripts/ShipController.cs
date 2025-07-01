using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class ShipController : MonoBehaviour, ISpaceship
{
    [SerializeField] private float SHIP_ACCELERATION = 10f;
    [SerializeField] private float SHIP_ANGULAR_ACCELERATION = 3f;

    [SerializeField] private CinemachineVirtualCamera mShipCamera;

    // Official passenger list
    private List<IPassenger> mCharactersOnboard = new List<IPassenger>();
    // Janky reference to character characters on board to start us off
    [SerializeField] cCharacterController[] mCharactersToStartBoarded;
    
    // Rigidbody reference
    Rigidbody2D mRigidbody;

    // Player input to steer ship
    private PlayerInput mPlayerControls;
    Vector2 mShipMoveInput;
    bool mDriverIsExiting;

    bool mIsDriving = false;

    private void Awake()
    {
        // Load control scheme
        mPlayerControls = new PlayerInput();
        // Handle thrust/yaw player input
        mPlayerControls.DriveShip.Move.performed += ctx => mShipMoveInput = ctx.ReadValue<Vector2>();
        mPlayerControls.DriveShip.Move.canceled += ctx => mShipMoveInput = Vector2.zero;
        // Handle interact input
        mPlayerControls.DriveShip.StopDriving.performed += ctx => mDriverIsExiting = true;
        mPlayerControls.DriveShip.StopDriving.canceled += ctx => mDriverIsExiting = false;
    }



    // Player boards the ship
    public void AddToCharactersOnBoard(IPassenger _Character)
    {
        if (_Character != null)
        {
            mCharactersOnboard.Add(_Character);
            _Character.BoardShip(this);
            Debug.Log($"Added {_Character.GetName()} to the ship. Total onboard: {mCharactersOnboard.Count}");
        }
        else
        {
            Debug.LogWarning("Attempted to add a null character to the ship.");
        }
    }

    // Return all characters aboard
    public List<IPassenger> GetCharactersOnboard()
    {
        return mCharactersOnboard;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public GameObject GetShip()
    {
        return gameObject;
    }

    public string GetShipName()
    {
        return gameObject.name;
    }

    // Player has left the ship
    public void PlayerDisembark(IPassenger _Character)
    {
        if (_Character != null && mCharactersOnboard.Contains(_Character))
        {
            mCharactersOnboard.Remove(_Character);
            _Character.DisembarkShip();
            Debug.Log($"Removed {_Character.GetName()} from the ship. Total onboard: {mCharactersOnboard.Count}");
        }
        else
        {
            Debug.LogWarning("Character not found on the ship or is null.");
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        // Get ship rigidbody
        mRigidbody = GetComponent<Rigidbody2D>();
        // set spaceship off soaring!
        mRigidbody.velocity = new Vector2(5, 0);

        // Add characters to passengerlist
        foreach (cCharacterController character in mCharactersToStartBoarded)
        {
            AddToCharactersOnBoard(character);
        }
    }

    private void Update()
    {
        if (mDriverIsExiting)
        {
            RemoveDriver();
        }
    }

    private void FixedUpdate()
    {
        if (mIsDriving)
        {
            // Move the ship according to driver input
            UpdateThrust();
            // Rotate ship according to driver input
            UpdateYaw();
            //UpdateCamera();
        }
    }

    private void UpdateThrust()
    {
        Vector2 ForceToAdd = new Vector2(mShipMoveInput.y * SHIP_ACCELERATION, 0);
        mRigidbody.AddRelativeForce(ForceToAdd);
    }

    private void UpdateYaw()
    {
        float ForceToAdd = (mShipMoveInput.x * -1) * SHIP_ANGULAR_ACCELERATION;
        mRigidbody.AddTorque(ForceToAdd);
    }

    public void PassengerDriveShip(IPassenger _Character)
    {
        mPlayerControls.Enable();
        _Character.OnPossessShip(this);
        mIsDriving = true;
        mShipCamera.Priority = 11;
    }

    public void RemoveDriver()
    {
        mPlayerControls.Disable();
        mCharactersToStartBoarded[0].StopDrivingShip();    // Janky player reference... player must be first character on board
        mIsDriving = false;
        mShipCamera.Priority = 9;
    }

    public float GetZRotation()
    {
        float rotationRadians = transform.eulerAngles.z;
        return rotationRadians;
    }
}
