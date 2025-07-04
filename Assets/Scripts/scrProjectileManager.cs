using System.Collections.Generic;
using UnityEngine;

public class scrProjectileManager : MonoBehaviour
{
    // Projectile Prefab
    [SerializeField] private GameObject mProjectilePrefab;
    [SerializeField] private int mProjectilesPerCharacterPool = 20;

    // Projectile pools
    SortedDictionary<IProjectileWeapon, List<GameObject>> mProjectilePools = new SortedDictionary<IProjectileWeapon, List<GameObject>>();




    public void AssignWeaponPool(IProjectileWeapon _Weapon)
    {
        // Don't create duplicate pools
        if (mProjectilePools.ContainsKey(_Weapon))
            return;

        // Create new pool
        List<GameObject> projectilePool = new List<GameObject>();

        // Add bullets to new projectile pool
        for (int i = 0; i < mProjectilesPerCharacterPool; i++)
        {
            GameObject projectile = Instantiate(mProjectilePrefab);
            projectile.SetActive(false); // Start inactive
            projectilePool.Add(projectile);
        }

        // Add pool to map of projectile pools
        mProjectilePools.Add(_Weapon, projectilePool);

        Debug.Log($"Created weapon pool for {_Weapon.GetWeaponName()}");
    }

    public GameObject GetPooledProjectile(IProjectileWeapon _Weapon)
    {
        if (!mProjectilePools.ContainsKey(_Weapon))
        {
            AssignWeaponPool(_Weapon);
        }

        List<GameObject> pool = mProjectilePools[_Weapon];

        // Find inactive projectile
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // If no inactive projectile found, create a new one
        GameObject newProjectile = Instantiate(mProjectilePrefab);
        newProjectile.SetActive(false);
        pool.Add(newProjectile);
        return newProjectile;
    }

    public void ReturnProjectileToPool(IProjectileWeapon _Weapon, GameObject _Projectile)
    {
        if (_Projectile != null)
        {
            _Projectile.SetActive(false);
            // Reset position to avoid clutter
            _Projectile.transform.position = Vector3.zero;

            // Reset velocity if it has a Rigidbody2D
            Rigidbody2D rb = _Projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    // Clean up unused pools when weapons are destroyed
    public void RemoveWeaponPool(IProjectileWeapon _Weapon)
    {
        if (mProjectilePools.ContainsKey(_Weapon))
        {
            List<GameObject> pool = mProjectilePools[_Weapon];

            // Destroy all projectiles in the pool
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                {
                    Destroy(pool[i]);
                }
            }

            // Remove the pool
            mProjectilePools.Remove(_Weapon);
        }
    }
}