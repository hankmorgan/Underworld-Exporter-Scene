using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * View-bobbing effect from UW1, from observation
 * There are 4 different kinds of view-bobbing:
 *  - Walk/Run
 *  - Strafe
 *  - Swim
 *  - Fly
 */
public class CameraBob : MonoBehaviour
{
    public enum Mode
    {
        NONE,
        WALK_FORWARD,
        STRAFE,
        SWIM,
        FLY
    }

    private Mode mode;
    // Default local Y value at standstill
    private float defaultY = 0.91f;
    // WALK_FORWARD and SWIM modes bob more intensely depending on move speed
    private float moveSpeed = 0.0f;
    // All bobs last for 1 second (although WALK_FORWARD uses abs() so it appears twice as fast)
    private float timer = 0.0f;
    private float BOB_DURATION = 1.0f;


    public void SetBobMode(Mode _mode)
    {
        mode = _mode;
    }

    public void SetSpeed(float _speed)
    {
        moveSpeed = _speed;
    }

    private void LateUpdate()
    {
        if (mode == Mode.NONE)
        {
            // Note: In the original UW the timer pauses when you stop moving,
            //       so the next time you start moving you will start part-way through the bob.
            //       Instead, we'll reset the timer here
            timer = 0.0f;
            return;
        }

        // All bob modes are based off a small sine wave
        float yOff = Mathf.Sin((Mathf.Deg2Rad * (360f * timer))) * 0.1f;
        float roll = 0;

        switch (mode)
        {
            case Mode.WALK_FORWARD:
                // abs() for bouncy walk, -0.03 to make it dip slightly below default
                yOff = Mathf.Abs(yOff) - 0.04f;
                // Amplify for move speed
                yOff *= moveSpeed * 0.5f;
                // Multiply negative values to add a punch to each step
                if (yOff < 0.0f) yOff *= 3.0f;
                break;
            case Mode.STRAFE:
                // Strafe bobs down and hangs at its peak for a moment
                // Invert the sine wave to start decending
                yOff = Mathf.Min(0, -(yOff + 0.06f)) * 2.0f;
                break;
            case Mode.SWIM:
                yOff *= 0.5f;
                yOff -= (defaultY + 0.5f);
                // roll based on move speed
                roll = Mathf.Sin((Mathf.Deg2Rad * (360f * (timer + 0.25f))));
                roll += (roll * moveSpeed);
                // Add a really fast mini roll as vibration
                roll += Random.Range(-2.0f, 2.0f);
                break;
            case Mode.FLY:
                // Just uses sine wave
                break;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, defaultY + yOff, transform.localPosition.z);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, roll);

        timer += Time.deltaTime;
        if (timer >= BOB_DURATION)
        {
            timer = 0;
        }
    }
}
