using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrHatchShipInteractable : MonoBehaviour, IShipInteractable
{
    [SerializeField] cBoardingHatch mHatchController;

    public bool CanInteract()
    {
        return true;
    }

    public void OnEnterInteractRange(IShipInteractable _Ship)
    {
        mHatchController.OpenHatch();
        mHatchController.SetConnectedShip(_Ship.GetShipController());
    }

    public void OnLeaveInteractRange(IShipInteractable _Ship)
    {
        mHatchController.CloseHatch();
        mHatchController.RemoveConnectedShip(_Ship.GetShipController());
    }
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ShipInteractable"))
        {
            OnEnterInteractRange(collision.GetComponent<IShipInteractable>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ShipInteractable"))
        {
            OnLeaveInteractRange(collision.GetComponent<IShipInteractable>());
        }
    }

    // IShipInteractable getter
    public cShipController GetShipController()
    {
        return mHatchController.GetShipOwner();
    }
}
