using UnityEngine;

public interface IWeaponProjectile
{
    void FireProjectile(Vector3 position, Vector3 direction, float speed, float damage);
    GameObject GetPooledProjectile();
    void ReturnProjectileToPool(GameObject projectile);
}