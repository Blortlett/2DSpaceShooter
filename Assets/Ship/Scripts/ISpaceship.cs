using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpaceship
{
    // Add character to list of boarded characters
    public void AddToCharactersOnBoard(IPassenger _Character);
    // Respond to character leaving ship
    public void PlayerDisembark(IPassenger _Character);
    // Get a passenger list
    public List<IPassenger> GetCharactersOnboard();
    // Get position of ship in the world
    public Vector2 GetPosition();
    // Get Z rotation of ship
    public float GetZRotation();
    // Get the ship gameobject
    public GameObject GetShip();
    // Get ship rigidbody
    public Rigidbody2D GetShipRigidbody();
    // Get boarding hatch
    public Transform GetBoardingHatch();
    // Get the name of the ship
    public string GetShipName();
    // Respond to a passenger taking control of the ship
    public void PassengerDriveShip(IPassenger _Character);
    // Respond to driver stops driving
    public void RemoveDriver();
}

public interface IPassenger : IComparable<IPassenger>
{
    // Character ID - character registered to aboard ship
    int Id { get; set;  }
    // How to respond to boarding a vessel
    public void BoardShip(ISpaceship _Character);
    // How to respond once character has comandeered the vessel
    public void OnPossessShip(ISpaceship _Ship);
    // Respond to player no longer driving ship
    public void StopDrivingShip();
    // How to respond to leaving the vessel
    public void DisembarkShip();
    // Get ship character is aboard, if aboard a ship
    public ISpaceship GetBoardedShip();
    // Get character type
    public cCharacterController.CharacterType GetCharacterType();
    // Get position inside ship
    public Vector2 GetPositionInsideShip();
}

public interface IShipInteractable
{
    public bool CanInteract();
    public void OnEnterInteractRange(IShipInteractable _Ship);
    public void OnLeaveInteractRange(IShipInteractable _Ship);
}