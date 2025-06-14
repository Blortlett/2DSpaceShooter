using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IPassenger
{
    // Constant values
    [SerializeField] float PLAYER_ACCELERATION = 5f;
    [SerializeField] float PLAYER_MAXSPEED = 1f;
    [SerializeField] float PLAYER_DRAG = 0.9f;

    // Inputs
    private PlayerInput mPlayerControls;
    private Vector2 mMoveInput;
    private Vector2 mMousePosition;
    private bool mIsInteracting;

    // Player physics
    private Vector2 mVelocity;

    // Current Ship Data
    bool IsAboardShip;
    private ISpaceship mCurrentBoardedSpaceship;
    private Vector2 mPositionInsideShip;

    // World Position
    private Vector2 mWorldPostion;

    [SerializeField] private Camera mPlayerCamera;

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
        UpdateCamera();
        RotateCharacter();
        UpdatePlayerPosition();
        MoveInput();
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
        // Apply velocity to character when inside ship
        mPositionInsideShip += mVelocity;
        Debug.Log("Player's currently boarded ship:" + mCurrentBoardedSpaceship.GetShipName());
        // Calculate world position
        mWorldPostion = mCurrentBoardedSpaceship.GetPosition() + mPositionInsideShip;
        
    }

    void UpdatePlayerPosition()
    {
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

    void UpdateCamera()
    {
        Vector3 NewPosition = new Vector3(mWorldPostion.x, mWorldPostion.y, mPlayerCamera.transform.position.z);
        mPlayerCamera.transform.position = NewPosition;
    }

    public void BoardShip(ISpaceship _Ship)
    {
        mCurrentBoardedSpaceship = _Ship;
    }

    public void DisembarkShip()
    {
        mCurrentBoardedSpaceship = null;
    }

    public ISpaceship GetBoardedShip()
    {
        return mCurrentBoardedSpaceship;
    }

    public string GetName()
    {
        return gameObject.name;
    }
}
