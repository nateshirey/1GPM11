using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Needle : MonoBehaviour
{
    public Rigidbody needleRb;

    private Vector2 inputValue;
    public bool pulledBack = false;

    public float maxPullbackMagnitude = 1f;
    public float pullbackAmount = 0;
    public float previousPullbackAmount = 0;
    public float throwAngle;

    public void OnPullback(InputValue value)
    {
        inputValue = value.Get<Vector2>();
        pulledBack = inputValue.magnitude > 0 ? true : false;
    }

    private void FixedUpdate()
    {
        PointNeedle();
        PositionNeedle();
    }

    private void Throw()
    {

    }

    private void PointNeedle()
    {
        throwAngle = Mathf.Atan2(inputValue.y, inputValue.x);
        throwAngle *= Mathf.Rad2Deg;
        if(throwAngle < 0)
        {
            throwAngle = 360 + throwAngle;
        }
        Quaternion rotation = Quaternion.Euler(0, 0, throwAngle - 180f);
        needleRb.gameObject.transform.rotation = rotation;
    }

    private void PositionNeedle()
    {
        Vector3 position = this.transform.position;
        if (pulledBack)
        {
            pullbackAmount = inputValue.magnitude;
            if(pullbackAmount < previousPullbackAmount)
            {
                pullbackAmount = previousPullbackAmount;
            }
            previousPullbackAmount = pullbackAmount;

            Vector2 clampedPos = inputValue.normalized;
            position += new Vector3(clampedPos.x * pullbackAmount * maxPullbackMagnitude, clampedPos.y * pullbackAmount * maxPullbackMagnitude, 0);

        }
        needleRb.gameObject.transform.position = position;
    }
}
