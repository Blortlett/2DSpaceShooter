using UnityEngine;
using UnityEngine.Tilemaps;

public class scrShipGraphicManager : MonoBehaviour
{
    [SerializeField] Tilemap mFloorTiles;
    [SerializeField] Tilemap mEngineTiles;
    [SerializeField] SpriteRenderer mShipExternalGraphics;

    private void Start()
    {
        SwapToExternalGraphics();
    }

    // put lerps n that in here when we're ready
    void Update()
    {
        
    }

    // Internal method to swap graphics
    private void SwapToInternalGraphics()
    {
        // Visible
        mFloorTiles.color = Color.white;
        mEngineTiles.color = Color.white;
        // Invisible
        mShipExternalGraphics.color = Color.clear;
    }

    // Internal method to swap graphics
    private void SwapToExternalGraphics()
    {
        // Invisible
        mFloorTiles.color = Color.clear;
        mEngineTiles.color = Color.clear;
        // Visible
        mShipExternalGraphics.color = Color.white;
    }

    // External toggle
    public void ToggleInternalGraphics()
    {
        SwapToInternalGraphics();
    }

    // External toggle
    public void ToggleExternalGraphics()
    {
        SwapToExternalGraphics();
    }
}
