using System.Collections.Generic;
using UnityEngine;

public class cInventory
{
    // Inventory List
    List<IProjectileWeapon> mInventoryList;
    
    public cInventory()
    {
        mInventoryList = new List<IProjectileWeapon>();
    }

    public void AddWeapon(IProjectileWeapon _Weapon)
    {
        mInventoryList.Add(_Weapon);
    }

    public IProjectileWeapon RetrieveWeapon(int _Index)
    {
        return mInventoryList[_Index];
    }

    public void DropWeapon(int _Index)
    {
        mInventoryList.RemoveAt(_Index);
    }
}