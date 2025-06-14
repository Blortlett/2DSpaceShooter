using System.Collections.Generic;
using UnityEngine;


public class ShipController : MonoBehaviour, ISpaceship
{
    // Official passenger list
    private List<IPassenger> mCharactersOnboard = new List<IPassenger>();
    // Janky reference to player character to start us off
    [SerializeField] PlayerController mPlayerCharacter;

    // Rigidbody reference
    Rigidbody2D mRigidbody;

    // Player boards the ship
    public void AddToCharactersOnBoard(IPassenger _Character)
    {
        if (_Character != null)
        {
            mCharactersOnboard.Add(_Character);
            _Character.BoardShip(this);
            Debug.Log($"Added {_Character.GetName()} to the ship. Total onboard: {mCharactersOnboard.Count}");
        }
        else
        {
            Debug.LogWarning("Attempted to add a null character to the ship.");
        }
    }

    // Return all characters aboard
    public List<IPassenger> GetCharactersOnboard()
    {
        return mCharactersOnboard;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public GameObject GetShip()
    {
        return gameObject;
    }

    public string GetShipName()
    {
        return gameObject.name;
    }

    // Player has left the ship
    public void PlayerDisembark(IPassenger _Character)
    {
        if (_Character != null && mCharactersOnboard.Contains(_Character))
        {
            mCharactersOnboard.Remove(_Character);
            _Character.DisembarkShip();
            Debug.Log($"Removed {_Character.GetName()} from the ship. Total onboard: {mCharactersOnboard.Count}");
        }
        else
        {
            Debug.LogWarning("Character not found on the ship or is null.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get ship rigidbody
        mRigidbody = GetComponent<Rigidbody2D>();
        // set spaceship off soaring!
        mRigidbody.velocity = new Vector2(5, 0);

        // Add player to passengerlist
        AddToCharactersOnBoard(mPlayerCharacter);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
