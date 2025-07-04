using UnityEngine;

public interface IProjectileWeapon
{
    GameObject GetPooledProjectile();
    void ReturnProjectileToPool(GameObject projectile);
    void StartFiring();
    void StopFiring();
    string GetWeaponName();
}