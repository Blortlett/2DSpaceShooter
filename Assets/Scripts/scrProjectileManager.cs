using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class scrProjectileManager : MonoBehaviour
{
    // Parent ship
    [SerializeField] private cShipController mParentShip;

    // Projectile Prefab
    [SerializeField] private GameObject mProjectilePrefab;
    [SerializeField] private int mProjectilesPerCharacterPool = 20;

    // Projectile pools
    SortedDictionary<IPassenger, List<GameObject>> mProjectilePools = new SortedDictionary<IPassenger, List<GameObject>>();

    private void Update()
    {
        foreach (List<GameObject> projectilePool in mProjectilePools.Values)
        {
            foreach(GameObject projectile in projectilePool)
            {
                projectile.UpdateBullet()
            }
        }
    }

    public void AssignPoolForCharacter(IPassenger _Passenger)
    {
        // Don't create duplicate pools
        if (mProjectilePools.ContainsKey(_Passenger))
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
        mProjectilePools.Add(_Passenger, projectilePool);

        Debug.Log($"Created weapon pool for {_Passenger.GetCharacterType()}");
    }

    public GameObject GetPooledProjectile(IPassenger _Passenger)
    {
        if (!mProjectilePools.ContainsKey(_Passenger))
        {
            AssignPoolForCharacter(_Passenger);
        }

        List<GameObject> pool = mProjectilePools[_Passenger];

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

    // Clean up unused pools when weapons are destroyed
    public void RemoveCharacterProjectilePool(IPassenger _Passenger)
    {
        if (mProjectilePools.ContainsKey(_Passenger))
        {
            List<GameObject> pool = mProjectilePools[_Passenger];

            // Destroy all projectiles in the pool
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                {
                    Destroy(pool[i]);
                }
            }

            // Remove the pool
            mProjectilePools.Remove(_Passenger);
        }
    }
}