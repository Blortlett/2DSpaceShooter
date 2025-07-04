using UnityEngine;

public interface IProjectileWeapon
{
    void FireProjectile(Vector3 position, Vector3 direction, float speed, float damage);
    GameObject GetPooledProjectile();
    void ReturnProjectileToPool(GameObject projectile);
}