using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class cControlStation : MonoBehaviour, IInteractable
{
    private ISpaceship mParentShip;

    public bool CanInteract()
    {
        return true;
    }

    public void OnInteract(cCharacterController _Character)
    {
        _Character.OnPossessShip(mParentShip);
        mParentShip.PassengerDriveShip(_Character);
    }

    private void Awake()
    {
        mParentShip = GetComponentInParent<ISpaceship>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
