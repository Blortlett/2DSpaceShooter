using System.Collections.Generic;
using UnityEngine;

public class cShipPassengerManager
{
    // Events for passenger changes
    public System.Action<IPassenger> OnPassengerBoarded;
    public System.Action<IPassenger> OnPassengerDisembarked;
    public System.Action<IPassenger> OnDriverChanged;
    public System.Action OnDriverRemoved;

    // Passenger storage
    private List<IPassenger> charactersOnboard = new List<IPassenger>();
    private IPassenger currentDriver;

    // Reference to ship for passenger callbacks
    private ISpaceship parentShip;

    public cShipPassengerManager(ISpaceship ship)
    {
        parentShip = ship;
    }

    #region Public Properties

    public IPassenger CurrentDriver => currentDriver;
    public List<IPassenger> CharactersOnboard => new List<IPassenger>(charactersOnboard); // Return copy for safety
    public int PassengerCount => charactersOnboard.Count;
    public bool HasDriver => currentDriver != null;
    public bool HasPassengers => charactersOnboard.Count > 0;

    #endregion

    #region Passenger Management

    public bool AddPassenger(IPassenger passenger)
    {
        if (passenger == null)
        {
            Debug.LogWarning("Attempted to add a null passenger to the ship.");
            return false;
        }

        if (charactersOnboard.Contains(passenger))
        {
            Debug.LogWarning($"Passenger {passenger.GetCharacterType()} is already on board.");
            return false;
        }

        charactersOnboard.Add(passenger);
        passenger.BoardShip(parentShip);
        OnPassengerBoarded?.Invoke(passenger);

        Debug.Log($"Added {passenger.GetCharacterType()} to the ship. Total onboard: {charactersOnboard.Count}");
        return true;
    }

    public bool RemovePassenger(IPassenger passenger)
    {
        if (passenger == null || !charactersOnboard.Contains(passenger))
        {
            Debug.LogWarning("Passenger not found on the ship or is null.");
            return false;
        }

        // If this passenger is the driver, remove them as driver first
        if (currentDriver == passenger)
        {
            RemoveDriver();
        }

        charactersOnboard.Remove(passenger);
        passenger.DisembarkShip();
        OnPassengerDisembarked?.Invoke(passenger);

        Debug.Log($"Removed {passenger.GetCharacterType()} from the ship. Total onboard: {charactersOnboard.Count}");
        return true;
    }

    public void RemoveAllPassengers()
    {
        // Remove driver first if exists
        if (currentDriver != null)
        {
            RemoveDriver();
        }

        // Remove all passengers
        var passengersToRemove = new List<IPassenger>(charactersOnboard);
        foreach (var passenger in passengersToRemove)
        {
            RemovePassenger(passenger);
        }
    }

    #endregion

    #region Driver Management

    public bool SetDriver(IPassenger passenger)
    {
        if (passenger == null)
        {
            Debug.LogWarning("Attempted to set null passenger as driver.");
            return false;
        }

        if (!charactersOnboard.Contains(passenger))
        {
            Debug.LogWarning("Cannot set driver: passenger is not on board.");
            return false;
        }

        if (currentDriver == passenger)
        {
            Debug.LogWarning("Passenger is already the driver.");
            return false;
        }

        // Remove current driver if exists
        if (currentDriver != null)
        {
            RemoveDriver();
        }

        currentDriver = passenger;
        passenger.OnPossessShip(parentShip);
        OnDriverChanged?.Invoke(passenger);

        Debug.Log($"{passenger.GetCharacterType()} is now driving the ship.");
        return true;
    }

    public void RemoveDriver()
    {
        if (currentDriver == null) return;

        var formerDriver = currentDriver;
        currentDriver.StopDrivingShip();
        currentDriver = null;
        OnDriverRemoved?.Invoke();

        Debug.Log($"{formerDriver.GetCharacterType()} stopped driving the ship.");
    }

    #endregion

    #region Query Methods

    public bool IsPassengerOnBoard(IPassenger passenger)
    {
        return passenger != null && charactersOnboard.Contains(passenger);
    }

    public bool HasPassengerOfType(cCharacterController.CharacterType characterType)
    {
        foreach (var passenger in charactersOnboard)
        {
            if (passenger.GetCharacterType() == characterType)
            {
                return true;
            }
        }
        return false;
    }

    public IPassenger GetFirstPassengerOfType(cCharacterController.CharacterType characterType)
    {
        foreach (var passenger in charactersOnboard)
        {
            if (passenger.GetCharacterType() == characterType)
            {
                return passenger;
            }
        }
        return null;
    }

    public List<IPassenger> GetPassengersOfType(cCharacterController.CharacterType characterType)
    {
        var result = new List<IPassenger>();
        foreach (var passenger in charactersOnboard)
        {
            if (passenger.GetCharacterType() == characterType)
            {
                result.Add(passenger);
            }
        }
        return result;
    }

    #endregion

    #region Utility Methods

    public void InitializeWithPassengers(IPassenger[] initialPassengers)
    {
        if (initialPassengers == null) return;

        foreach (var passenger in initialPassengers)
        {
            AddPassenger(passenger);
        }
    }

    #endregion
}