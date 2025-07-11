using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cBoardingHatch : MonoBehaviour
{
    private cShipController mShipOwner;
    private cShipController mConnectedShip;

    private bool isHatchOpen = false;
    [SerializeField] private SpriteRenderer mShipHatchRenderer;
    [SerializeField] private Sprite mOpenHatchGraphic;
    [SerializeField] private Sprite mClosedHatchGraphic;

    private void Start()
    {
        mShipOwner = GetComponentInParent<cShipController>();
        mShipOwner.AddBoardingHatch(this);
        CloseHatch();
    }

    public void OpenHatch()
    {
        Debug.Log("Opening Hatch");
        mShipHatchRenderer.sprite = mOpenHatchGraphic;
        isHatchOpen = true;
    }

    public void CloseHatch()
    {
        Debug.Log("Closing Hatch");
        mShipHatchRenderer.sprite = mClosedHatchGraphic;
        isHatchOpen = false;
    }

    // -= Setters =-
    public void SetConnectedShip(cShipController _ConnectedShip) { mConnectedShip = _ConnectedShip; }
    public void RemoveConnectedShip(cShipController _ConnectedShip)
    {
        if (mConnectedShip == _ConnectedShip)
        {
            mConnectedShip = null;
        }
    }

    // -= Getters =-
    public bool GetIsHatchOpen() { return isHatchOpen; }
    public cShipController GetShipOwner() { return mShipOwner; }
    public cShipController GetConnectedShip() { return mConnectedShip; }
}