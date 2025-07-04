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
        Debug.Log($"{_Character.GetCharacterType()} interacted with {(mHatchController.GetIsHatchOpen() ? "open" : "closed")} boarding hatch");
    }
}
