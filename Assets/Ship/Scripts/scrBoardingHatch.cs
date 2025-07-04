using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cBoardingHatch : MonoBehaviour
{
    private cShipController mShipOwner;

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

    // -= Getters =-
    public bool GetIsHatchOpen() { return true; }
}