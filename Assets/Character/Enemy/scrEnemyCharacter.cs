using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    private cCharacterController mCharacterController;

    // "Input" Variables
    private Vector2 mInputDirection = new Vector2(1, 0);

    // Patrol variables
    private bool mIsPatrolling = false;
    private float mPatrolTimeMax = 2f;
    private float mPatrolTimer;

    // Start is called before the first frame update
    void Start()
    {
        // Get atached charController.
        mCharacterController = GetComponent<cCharacterController>();

        // reset timers
        mPatrolTimer = mPatrolTimeMax;
    }

    // Update is called once per frame
    void Update()
    {
        // tick down timer
        mPatrolTimer -= Time.deltaTime;
        if (mPatrolTimer < 0)
        {
            // Swap patrol/wait state
            mIsPatrolling = !mIsPatrolling;
            // Reset patrol timer
            mPatrolTimer = mPatrolTimeMax;
            // Swap move direction on restart patrol
            if (mIsPatrolling)
            {
                // Flip move direction
                mInputDirection = new Vector2(mInputDirection.x * -1, 0);
            }
        }

        // Execute patrol move logic
        if (mIsPatrolling)
        {   // Enemy Walking
            mCharacterController.MoveInput(mInputDirection);
            mCharacterController.RotateCharacter(mCharacterController.GetPosition() + mInputDirection * 5);
        }
        else
        {   // Enemy Waiting
            mCharacterController.MoveInput(Vector2.zero);
        }


    }
}
