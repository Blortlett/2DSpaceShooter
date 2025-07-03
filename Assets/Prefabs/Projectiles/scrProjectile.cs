using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class scrProjectile : MonoBehaviour
{
    // Movement variables
    private Vector2 mLocalPosition = Vector2.zero;
    private Vector2 mTrajectory = Vector2.zero;
    private float mSpeed = 300;

    // Alive for Timer
    [SerializeField] private float mAliveForTimerMax = 5f;
    private float mAliveForTimer;

    // Object pool selects this to fire bullet
    private void FireProjectile(Vector2 _LocalStartPosition, Vector2 _Traejctory, float _Speed)
    {
        mLocalPosition = _LocalStartPosition;
        mTrajectory = _Traejctory;
        mAliveForTimer = mAliveForTimerMax;
    }

    void Start()
    {
        // Set inactive wait to be activated in object pool
        gameObject.SetActive(false);
    }

    void Update()
    {
        // tick down timer
        mAliveForTimer -= Time.deltaTime;
        if (mAliveForTimer < 0f)
        {
            // Reset projectile
            mTrajectory = Vector2.zero;
            mAliveForTimer = mAliveForTimerMax;
            // Disable projectile
            gameObject.SetActive(false);
            return;
        }

        // Move towards trajectory at speed
        float newXPos = transform.position.x + mTrajectory.x * mSpeed;
        float newYPos = transform.position.y + mTrajectory.y * mSpeed;
        transform.position = new Vector2(newXPos , newYPos);
    }
}
