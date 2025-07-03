using System.Collections.Generic;
using UnityEngine;

public class scrProjectileManager : MonoBehaviour
{
    [SerializeField] private GameObject mProjectilePrefab;
    [SerializeField] private int mProjectilesPerCharacterPool = 20;

    // Projectile pools
    SortedDictionary<IWeaponProjectile, List<GameObject>> mProjectilePools = new SortedDictionary<IWeaponProjectile, List<GameObject>>();




    public void AssignWeaponPool(IWeaponProjectile _Weapon)
    {
        // Create new pool
        List<GameObject> projectilePool = new List<GameObject>();
        // Add bullets to new projectile pool
        for (int i = 0; i < mProjectilesPerCharacterPool; i++)
        {
            projectilePool.Add(Instantiate(mProjectilePrefab));
        }
        // Add pool to map of projectile pools
        mProjectilePools.Add(_Weapon, projectilePool);
    }

    
    void Update()
    {
        
    }
}
