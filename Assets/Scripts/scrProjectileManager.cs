using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class scrProjectileManager : MonoBehaviour
{
    // Parent ship
    [SerializeField] private cShipController mParentShip;

    // Projectile Prefab
    [SerializeField] private GameObject mProjectilePrefab;
    [SerializeField] private int mProjectilesPerCharacterPool = 20;

    // Projectile pools
    SortedDictionary<IPassenger, List<scrProjectile>> mProjectilePools = new SortedDictionary<IPassenger, List<scrProjectile>>();

    private void Update()
    {
        foreach (List<scrProjectile> projectilePool in mProjectilePools.Values)
        {
            foreach(scrProjectile projectile in projectilePool)
            {
                projectile.UpdateBullet(mParentShip.GetPosition());
            }
        }
    }

    public void AssignPoolForCharacter(IPassenger _Passenger)
    {
        // Don't create duplicate pools
        if (mProjectilePools.ContainsKey(_Passenger))
            return;

        // Create new pool
        List<scrProjectile> projectilePool = new List<scrProjectile>();

        // Add bullets to new projectile pool
        for (int i = 0; i < mProjectilesPerCharacterPool; i++)
        {
            scrProjectile projectile = Instantiate(mProjectilePrefab).GetComponent<scrProjectile>();
            projectile.gameObject.SetActive(false); // Start inactive
            projectilePool.Add(projectile);
        }

        // Add pool to map of projectile pools
        mProjectilePools.Add(_Passenger, projectilePool);

        Debug.Log($"Created weapon pool for {_Passenger.GetCharacterType()}");
    }

    public scrProjectile GetPooledProjectile(IPassenger _Passenger)
    {
        if (!mProjectilePools.ContainsKey(_Passenger))
        {
            AssignPoolForCharacter(_Passenger);
        }

        List<scrProjectile> pool = mProjectilePools[_Passenger];

        // Find inactive projectile
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeInHierarchy)
            {
                return pool[i];
            }
        }


        Debug.LogError("Fired too many bullets");
        return null;
    }

    // Clean up unused pools when weapons are destroyed
    public void RemoveCharacterProjectilePool(IPassenger _Passenger)
    {
        if (mProjectilePools.ContainsKey(_Passenger))
        {
            List<scrProjectile> pool = mProjectilePools[_Passenger];

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