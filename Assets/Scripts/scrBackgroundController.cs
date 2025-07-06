using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class scrBackgroundController : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Renderer layerRenderer;     // Renderer of the quad using the texture
        public float parallaxFactor;     // How much the background moves relative to the camera
    }

    public ParallaxLayer[] layers;
    public Transform target;               // Typically your player or camera
    private Vector3 lastPosition;

    void Start()
    {
        if (target == null)
            target = Camera.main.transform;

        lastPosition = target.position;
    }

    void LateUpdate()
    {
        transform.position = target.transform.position;

        Vector3 deltaMovement = target.position - lastPosition;

        foreach (var layer in layers)
        {
            Vector2 offset = layer.layerRenderer.material.mainTextureOffset;
            offset -= new Vector2(deltaMovement.x, deltaMovement.y) * layer.parallaxFactor;
            layer.layerRenderer.material.mainTextureOffset = offset;
        }

        lastPosition = target.position;
    }
}