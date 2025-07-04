using UnityEngine;

public class scrProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private LayerMask mTargetLayers = -1;
    [SerializeField] private bool mDestroyOnHit = true;
    [SerializeField] private GameObject mHitEffect;
    [SerializeField] private float mLifetime = 5f;

    private Vector3 mDirection;
    private float mSpeed;
    private float mDamage;
    private float mMaxRange;
    private Vector3 mStartPosition;
    private IProjectileWeapon mSourceWeapon;
    private Rigidbody2D mRigidbody; // Do we need this? Delete if can plz
    private float mActiveTime;

    void Awake()
    {
        mRigidbody = GetComponent<Rigidbody2D>();
        if (mRigidbody == null)
        {
            mRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        // Set up rigidbody for projectile physics
        mRigidbody.gravityScale = 0f; // No gravity for top-down
        mRigidbody.drag = 0f;
        mRigidbody.angularDrag = 0f;
    }

    void OnEnable()
    {
        mActiveTime = 0f;
    }

    void Update()
    {
        mActiveTime += Time.deltaTime;

        // Check lifetime
        if (mActiveTime >= mLifetime)
        {
            ReturnToPool();
            return;
        }

        // Check range
        if (mMaxRange > 0 && Vector3.Distance(mStartPosition, transform.position) >= mMaxRange)
        {
            ReturnToPool();
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit a valid target
        if (((1 << other.gameObject.layer) & mTargetLayers) != 0)
        {
            // Deal damage if the target has a health component
            var healthComponent = other.GetComponent<IHealth>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(mDamage);
            }

            // Spawn hit effect
            if (mHitEffect != null)
            {
                Instantiate(mHitEffect, transform.position, Quaternion.identity);
            }

            // Return to pool or destroy
            if (mDestroyOnHit)
            {
                ReturnToPool();
            }
        }
    }

    public void Initialize(Vector3 direction, float speed, float damage, float maxRange, IProjectileWeapon sourceWeapon)
    {
        mDirection = direction.normalized;
        mSpeed = speed;
        mDamage = damage;
        mMaxRange = maxRange;
        mSourceWeapon = sourceWeapon;
        mStartPosition = transform.position;

        // Set velocity
        if (mRigidbody != null)
        {
            mRigidbody.velocity = mDirection * mSpeed;
        }

        // Rotate to face direction
        if (mDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(mDirection.y, mDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void ReturnToPool()
    {
        if (mSourceWeapon != null)
        {
            mSourceWeapon.ReturnProjectileToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

// Optional interface for objects that can take damage
public interface IHealth
{
    void TakeDamage(float damage);
    float GetCurrentHealth();
    float GetMaxHealth();
}