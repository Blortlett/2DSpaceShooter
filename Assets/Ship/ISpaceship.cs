using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpaceship
{
    public void AddToCharactersOnBoard(IPassenger _Character);
    public void PlayerDisembark(IPassenger _Character);
    public List<IPassenger> GetCharactersOnboard();
    public Vector2 GetPosition();
    public GameObject GetShip();
    public string GetShipName();
    public void PassengerDriveShip(IPassenger _Character);
}

public interface IPassenger
{
    public void BoardShip(ISpaceship _Character);
    public void DisembarkShip();
    public ISpaceship GetBoardedShip();
    public string GetName();
    
    public void OnPossessShip(ISpaceship _Ship);
}