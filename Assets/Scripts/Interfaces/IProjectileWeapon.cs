using UnityEngine;

public interface IProjectileWeapon
{
    scrProjectile GetPooledProjectile();
    void StartFiring();
    void StopFiring();
    string GetWeaponName();
    void Pickup(IPassenger _Character);
    void Drop();
}