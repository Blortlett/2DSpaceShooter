using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrHatchShipInteractable : MonoBehaviour
{
    [SerializeField] cBoardingHatch mHatchController;

    public bool CanInteract()
    {
        return true;
    }

    public void OnEnterInteractRange(IShipInteractable _Ship)
    {
        mHatchController.OpenHatch();
    }

    public void OnLeaveInteractRange(IShipInteractable _Ship)
    {
        mHatchController.CloseHatch();
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
}
