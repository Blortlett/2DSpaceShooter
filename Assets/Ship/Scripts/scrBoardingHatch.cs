using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cBoardingHatch : MonoBehaviour
{
    private cShipController mShipOwner;

    private void Start()
    {
        mShipOwner = GetComponentInParent<cShipController>();
        mShipOwner.AddBoardingHatch(this);
    }
}
