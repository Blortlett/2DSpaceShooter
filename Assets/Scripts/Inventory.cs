using System.Collections.Generic;
using UnityEngine;

public class cInventory
{
    // Inventory List
    List<IWeaponProjectile> mInventoryList;
    
    public cInventory()
    {
        mInventoryList = new List<IWeaponProjectile>();
    }

    public void AddWeapon(IWeaponProjectile _Weapon)
    {
        mInventoryList.Add(_Weapon);
    }

    public IWeaponProjectile RetrieveWeapon(int _Index)
    {
        return mInventoryList[_Index];
    }

    public void DropWeapon(int _Index)
    {
        mInventoryList.RemoveAt(_Index);
    }
}