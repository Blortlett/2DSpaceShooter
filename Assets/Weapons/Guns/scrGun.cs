using System.Collections;
using UnityEngine;

public enum FireMode
{
    SemiAutomatic,
    Automatic
}

public class scrGun : MonoBehaviour, IInteractable, IProjectileWeapon
{
    [Header("Gun Settings")]
    [SerializeField] private string mGunName = "Pistol";
    [SerializeField] private FireMode mFireMode = FireMode.SemiAutomatic;
    [SerializeField] private float mFireRate = 600f; // Rounds per minute
    [SerializeField] private float mDamage = 25f;
    [SerializeField] private float mProjectileSpeed = 15f;
    [SerializeField] private int mMagazineSize = 12;
    [SerializeField] private float mReloadTime = 2f;
    [SerializeField] private float mRange = 20f;

    [Header("Visual/Audio")]
    [SerializeField] private Transform mFirePoint;
    [SerializeField] private ParticleSystem mMuzzleFlash;
    [SerializeField] private AudioClip mFireSound;
    [SerializeField] private AudioClip mReloadSound;
    [SerializeField] private AudioClip mEmptySound;

    [Header("Recoil")]
    [SerializeField] private float mRecoilAmount = 0.1f;
    [SerializeField] private float mRecoilRecoverySpeed = 5f;

    // Internal state
    private int mCurrentAmmo;
    private float mNextFireTime;
    private bool mIsReloading;
    private bool mTriggerHeld;
    private cCharacterController mCurrentHolder;
    private scrProjectileManager mProjectileManager;
    private AudioSource mAudioSource;

    // Recoil
    private Vector3 mOriginalPosition;
    private Vector3 mRecoilOffset;

    void Start()
    {
        mCurrentAmmo = mMagazineSize;
        mProjectileManager = FindObjectOfType<scrProjectileManager>();
        mAudioSource = GetComponent<AudioSource>();

        if (mAudioSource == null)
        {
            mAudioSource = gameObject.AddComponent<AudioSource>();
        }

        mOriginalPosition = transform.localPosition;

        // Register this weapon with the projectile manager
        if (mProjectileManager != null)
        {
            mProjectileManager.AssignWeaponPool(this);
        }
    }

    void Update()
    {
        // Handle recoil recovery
        if (mRecoilOffset.magnitude > 0.01f)
        {
            mRecoilOffset = Vector3.Lerp(mRecoilOffset, Vector3.zero, mRecoilRecoverySpeed * Time.deltaTime);
            transform.localPosition = mOriginalPosition + mRecoilOffset;
        }

        // Handle automatic fire
        if (mCurrentHolder != null && mTriggerHeld && mFireMode == FireMode.Automatic)
        {
            TryFire();
        }
    }

    #region IInteractable Implementation
    public bool CanInteract()
    {
        return mCurrentHolder == null; // Can only be picked up if not already held
    }

    public void OnInteract(cCharacterController _Character)
    {
        if (CanInteract())
        {
            PickUp(_Character);
        }
    }
    #endregion

    #region IWeaponProjectile Implementation
    public void FireProjectile(Vector3 position, Vector3 direction, float speed, float damage)
    {
        GameObject projectile = GetPooledProjectile();
        if (projectile != null)
        {
            projectile.transform.position = position;
            projectile.transform.right = direction; // Assuming projectiles face right
            projectile.SetActive(true);

            // Configure projectile (assuming it has a projectile script)
            var projectileScript = projectile.GetComponent<scrProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, speed, damage, mRange, this);
            }
            else
            {
                // Fallback: use Rigidbody2D if no custom projectile script
                var rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = direction * speed;
                }
            }
        }
    }

    public GameObject GetPooledProjectile()
    {
        if (mProjectileManager != null)
        {
            return mProjectileManager.GetPooledProjectile(this);
        }
        return null;
    }

    public void ReturnProjectileToPool(GameObject projectile)
    {
        if (mProjectileManager != null)
        {
            mProjectileManager.ReturnProjectileToPool(this, projectile);
        }
        else
        {
            projectile.SetActive(false);
        }
    }
    #endregion

    #region Gun Control Methods
    public void PickUp(cCharacterController character)
    {
        mCurrentHolder = character;
        transform.SetParent(character.transform);
        // Position the gun relative to the character
        transform.localPosition = Vector3.zero;
        mOriginalPosition = transform.localPosition;
    }

    public void Drop()
    {
        mCurrentHolder = null;
        transform.SetParent(null);
        mTriggerHeld = false;
    }

    public void StartFiring()
    {
        mTriggerHeld = true;

        if (mFireMode == FireMode.SemiAutomatic)
        {
            TryFire();
        }
    }

    public void StopFiring()
    {
        mTriggerHeld = false;
    }

    private void TryFire()
    {
        if (mIsReloading || Time.time < mNextFireTime)
            return;

        if (mCurrentAmmo <= 0)
        {
            PlaySound(mEmptySound);
            return;
        }

        Fire();
    }

    private void Fire()
    {
        mCurrentAmmo--;
        mNextFireTime = Time.time + (60f / mFireRate);

        // Calculate fire direction based on gun's rotation
        Vector3 fireDirection = transform.right;
        Vector3 firePosition = mFirePoint ? mFirePoint.position : transform.position;

        // Fire projectile
        FireProjectile(firePosition, fireDirection, mProjectileSpeed, mDamage);

        // Visual effects
        if (mMuzzleFlash != null)
        {
            mMuzzleFlash.Play();
        }

        // Audio
        PlaySound(mFireSound);

        // Recoil
        ApplyRecoil();

        // Auto-reload if empty
        if (mCurrentAmmo <= 0)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private void ApplyRecoil()
    {
        mRecoilOffset = -transform.right * mRecoilAmount;
    }

    public void Reload()
    {
        if (!mIsReloading && mCurrentAmmo < mMagazineSize)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        mIsReloading = true;
        PlaySound(mReloadSound);

        yield return new WaitForSeconds(mReloadTime);

        mCurrentAmmo = mMagazineSize;
        mIsReloading = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (mAudioSource != null && clip != null)
        {
            mAudioSource.PlayOneShot(clip);
        }
    }
    #endregion

    #region Public Properties
    public string GunName => mGunName;
    public FireMode CurrentFireMode => mFireMode;
    public int CurrentAmmo => mCurrentAmmo;
    public int MagazineSize => mMagazineSize;
    public bool IsReloading => mIsReloading;
    public bool HasAmmo => mCurrentAmmo > 0;
    public cCharacterController CurrentHolder => mCurrentHolder;
    #endregion
}