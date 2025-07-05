using UnityEngine;

public class scrProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private LayerMask mTargetLayers = -1;
    [SerializeField] private bool mDestroyOnHit = true;
    [SerializeField] private GameObject mHitEffect;
    [SerializeField] private float mLifetime = 5f;

    private bool mIsBulletActive;
    private Vector2 mDirection;
    private float mSpeed;
    private float mDamage;
    private float mMaxRange;
    private Vector2 mPositionInShip;
    private IProjectileWeapon mSourceWeapon;
    private float mActiveTime;

    void Awake()
    {
        
    }

    void OnEnable()
    {
        mActiveTime = 0f;
    }

    public void UpdateBullet(Vector2 _ShipWorldPosition)
    {
        // Don't update unless bullet is active
        if (!mIsBulletActive) return;

        // Move Bullet
        mPositionInShip += mDirection * mSpeed * Time.deltaTime;

        // calculate and set bullet's world position
        transform.position = _ShipWorldPosition + mPositionInShip;
        // Lifetime timer
        mActiveTime += Time.deltaTime;
        // Check lifetime
        if (mActiveTime >= mLifetime)
        {
            ReturnToPool();
            return;
        }
    }

    // Bullet hit object
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bullet Hit {other.gameObject.name}");

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

    // Fire bullet
    public void Initialize(Vector2 localStartPosition, Vector2 direction, float speed, float damage, float maxRange, IProjectileWeapon sourceWeapon)
    {
        mIsBulletActive = true;
        mDirection = direction.normalized;
        mSpeed = speed;
        mDamage = damage;
        mMaxRange = maxRange;
        mSourceWeapon = sourceWeapon;
        mPositionInShip = localStartPosition;

        Debug.Log($"Bullet fired @{mPositionInShip.x}x{mPositionInShip.y}");
    }

    private void ReturnToPool()
    {
        mIsBulletActive = false;
        gameObject.SetActive(false);
    }
}

// Optional interface for objects that can take damage
public interface IHealth
{
    void TakeDamage(float damage);
    float GetCurrentHealth();
    float GetMaxHealth();
}