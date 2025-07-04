using System.Collections.Generic;
using UnityEngine;

public class cInventory
{
    // Inventory List
    List<IProjectileWeapon> mInventoryList;
    //IProjectileWeapon mEquippedWeapon;
    
    public cInventory()
    {
        mInventoryList = new List<IProjectileWeapon>();
    }

    public void AddWeapon(IProjectileWeapon _Weapon)
    {
        mInventoryList.Add(_Weapon);
        //if (mEquippedWeapon == null)
        //{
        //    mEquippedWeapon = _Weapon;
        //}
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