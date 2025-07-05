using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class cCharacterController : MonoBehaviour, IPassenger, IComparable<IPassenger>
{
    public enum CharacterType {
        NeutralNPC,
        Player,
        EnemyNPC,
    }

    public int Id { get; set; } // Example property for comparison
    // Other properties and methods

    public int CompareTo(IPassenger other)
    {
        if (other == null) return 1; // Null is considered less than any instance
        return this.Id.CompareTo(other.Id); // Compare based on Id (or another property)
    }

    // Constant values
    [SerializeField] float CHARACTER_ACCELERATION = 5f;
    [SerializeField] float CHARACTER_MAXSPEED = 1f;
    [SerializeField] float CHARACTER_DRAG = 0.9f;

    // Events
    public event EventHandler OnStartDrivingShip;
    public event EventHandler OnStopDrivingShip;

    // Character type
    [SerializeField] private CharacterType mCharacterType;

    // Character physics
    private Vector2 mVelocity;
    private Rigidbody2D mRigidBody;

    // Wall collisions
    private bool mIsCollidingWithWall = false;
    private Vector2 mWallNormal;

    // Current Ship Data
    private ISpaceship mCurrentBoardedSpaceship;
    private Vector2 mPositionInsideShip;

    // World Position
    private Vector2 mWorldPostion;

    // Latest interactable nearby player
    private List<IInteractable> mInteractablesNearby = new List<IInteractable>();

    // Character Inventory
    private cInventory mInventory;
    private IProjectileWeapon mEquippedWeapon;

    private void Awake()
    {
        mInventory = new cInventory();
    }

    void Start()
    {
        // Get Rigidbody ref
        mRigidBody = GetComponent<Rigidbody2D>();

        // -= Weapon stuff =-
        // If a weapon is a child of the character on start, pick up the first weapon
        IProjectileWeapon checkHoldingWeapon = GetComponentInChildren<IProjectileWeapon>();
        if (checkHoldingWeapon != null)
        {
            mInventory.AddWeapon(checkHoldingWeapon);
            // Equip this weapon
            mEquippedWeapon = checkHoldingWeapon;
            mEquippedWeapon.Pickup(this);
            Debug.Log($"{mCharacterType} is holding weapon: {checkHoldingWeapon.GetWeaponName()}");

        }
    }

    // Input n that
    private void Update()
    {
        //HandleInteract();
        UpdatePlayerWorldPosition();
    }

    // Physics stuff
    void FixedUpdate()
    {
        MoveInsideShip();
    }

    public void PickupWeapon(IProjectileWeapon _Weapon)
    {
        mInventory.AddWeapon(_Weapon);
    }

    // Called from the controller - (Enemy, Player or NeutralNPC)
    // Takes in a normalized vector 2 and calculates player movement
    public void MoveInput(Vector2 _NormalizedInput)
    {
        // Apply acceleration to input
        Vector2 AddVelocity = _NormalizedInput * CHARACTER_ACCELERATION;
        mVelocity += AddVelocity * Time.deltaTime;

        // Apply drag
        mVelocity *= Mathf.Pow(CHARACTER_DRAG, Time.deltaTime);

        // Clamp max velocity
        if (mVelocity.magnitude > CHARACTER_MAXSPEED)
        {
            mVelocity = mVelocity.normalized * CHARACTER_MAXSPEED;
        }
    }

    // called in update()
    void MoveInsideShip()
    {
        // Get allowed velocity (handles wall collision)
        Vector2 allowedVelocity = GetAllowedMovement(mVelocity);

        // Apply rotated velocity to character position inside ship
        mPositionInsideShip += allowedVelocity * Time.fixedDeltaTime;
    }

    void UpdatePlayerWorldPosition()
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
        mRigidBody.position = mWorldPostion;
    }

    public void RotateCharacter(Vector2 _LookAtPosition)
    {
        // Convert mouse screen position to world position in 2D
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(_LookAtPosition);

        // Calculate direction from player to mouse
        Vector2 direction = mouseWorldPos - (Vector2)transform.position;

        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation around Z-axis (for 2D top-down)
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    public void PullWeaponTrigger()
    {
        mEquippedWeapon.StartFiring();
    }

    public void ReleaseWeaponTrigger()
    {
        mEquippedWeapon.StopFiring();
    }

    public void HandleInteract()
    {
        // only if there are items nearby
        if (mInteractablesNearby.Count > 0)
        {   // interact with first item on list
            mInteractablesNearby[0].OnInteract(this);
        }
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

    public void OnPossessShip(ISpaceship _Ship)
    {
        OnStartDrivingShip?.Invoke(this, EventArgs.Empty);
    }

    public void StopDrivingShip()
    {
        OnStopDrivingShip?.Invoke(this, EventArgs.Empty);
    }

    Vector2 GetAllowedMovement(Vector2 vel)
    {
        if (!mIsCollidingWithWall)
            return vel; // No collision, allow full movement

        // Calculate dot product
        float dot = Vector2.Dot(vel, mWallNormal);

        // If dot product is positive, movement is toward the wall
        if (dot > 0)
        {
            // Project velocity onto the plane perpendicular to the wall normal (sliding)
            Vector2 wallTangent = new Vector2(-mWallNormal.y, mWallNormal.x); // Perpendicular to normal
            float tangentComponent = Vector2.Dot(vel, wallTangent);
            return wallTangent * tangentComponent; // Allow only the component along the wall
        }

        // Movement is parallel or away from the wall, allow it fully
        return vel;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player touching wall!");
        // Check if player is colliding with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            mIsCollidingWithWall = true;
            // Get the normal of the contact point (average if multiple contacts)
            mWallNormal = Vector2.zero;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                mWallNormal += contact.normal;
            }
            mWallNormal = mWallNormal.normalized;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Player still is colliding with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Update wall normal during continuous collision
            mWallNormal = Vector2.zero;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                mWallNormal += contact.normal;
            }
            mWallNormal = mWallNormal.normalized;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if player stopped colliding with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            mIsCollidingWithWall = false;
        }
    }

    // -= Setters =-
    public void SetCharacterType(CharacterType _CharacterType) { mCharacterType = _CharacterType; }

    // -= Getters =-
    public Vector2 GetPosition() { return transform.position; }

    public CharacterType GetCharacterType()
    {
        return mCharacterType;
    }

}
