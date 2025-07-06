using UnityEngine;

public class scrHatchPlayerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] cBoardingHatch mHatchController;

    public bool CanInteract()
    {
        return true; // mHatchController.GetIsHatchOpen();  // <- player only able to interact when hatch is open?
    }

    public void OnInteract(cCharacterController _Character)
    {
        // Debug output
        Debug.Log($"{_Character.GetCharacterType()} interacted with {(mHatchController.GetIsHatchOpen() ? "open" : "closed")} boarding hatch");

        // Return if hatch is not connected
        if (!mHatchController.GetIsHatchOpen()) return;

        // Ship is connected, transfer player to other ship
        mHatchController.GetShipOwner().PlayerDisembark(_Character); // Leave current ship
        mHatchController.GetConnectedShip().AddToCharactersOnBoard(_Character); // Enter new ship
    }
}
