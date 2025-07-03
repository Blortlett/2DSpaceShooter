using System.Collections.Generic;
using UnityEngine;

public class scrProjectileManager : MonoBehaviour
{
    [SerializeField] private GameObject mProjectilePrefab;
    [SerializeField] private int mProjectilesPerCharacterPool = 20;

    // List of characters to pool for
    [SerializeField] List<cCharacterController.CharacterType> Characters = new List<cCharacterController.CharacterType>();
    // Projectile pools
    List<List<GameObject>> mProjectilePools = new List<List<GameObject>>();
    


    public void AddPassengerToManager(IPassenger _Passenger)
    {
        // Save Character type reference... maybe not so needed?
        cCharacterController.CharacterType characterType = _Passenger.GetCharacterType();
        Characters.Add(characterType);
        // Create new pool
        List<GameObject> projectilePool = new List<GameObject>();
        // Add bullets to new projectile pool
        for (int i = 0; i < mProjectilesPerCharacterPool; i++)
        {
            projectilePool.Add(Instantiate(mProjectilePrefab));
        }
        // Add pool to list of projectile pools
        mProjectilePools.Add(projectilePool);
    }

    
    void Update()
    {
        
    }
}
