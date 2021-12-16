using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class NeedleInput : MonoBehaviour
{
    private Camera cam;

    public NeedleCollision needleCollision;

    private Vector2 inputValue;
    public bool pulledBack = false;

    [Range(0.5f, 10f)]
    public float throwStrength = 1f;

    public bool canThrow = false;

    public float maxPullbackMagnitude = 1f;
    public float pullbackAmount = 0;
    public float previousPullbackAmount = 0;
    public float throwAngle;
    [Range(0.1f, 0.5f)]
    public float mouseNormalizeRadius = 0.2f;
    private float mouseNormalRatio;

    public Vector2 mousePos;

    private void Awake()
    {
        cam = Camera.main;
        mouseNormalRatio = 1f / mouseNormalizeRadius;
    }

    //only used for controller input
    public void OnPullback(InputValue value)
    {
        inputValue = value.Get<Vector2>();
        pulledBack = inputValue.magnitude > 0 ? true : false;
    }


    //only used for mouse/keyboard input
    public void OnMousePosition(InputValue value)
    {
        mousePos = value.Get<Vector2>();
        Vector2 playerPos = cam.WorldToScreenPoint(this.transform.position);
        mousePos -= playerPos;
        mousePos /= cam.scaledPixelWidth;
        mousePos.y *= cam.aspect;

        mousePos *= mouseNormalRatio;
        mousePos = Vector2.ClampMagnitude(mousePos, 1f);

        inputValue = mousePos;
    }

    public void OnMouseClick(InputValue value)
    {
        pulledBack = value.isPressed ? true : false;
        if (pulledBack)
        {
            canThrow = true;
        }
        //should only be called the frame that the click is released
        else if (canThrow)
        {
            canThrow = false;
            needleCollision.ThrowNeedle(throwStrength);
        }
    }

    private void FixedUpdate()
    {
        switch (needleCollision.needleState)
        {
            case NeedleState.Held:
                PointNeedle();
                PositionNeedle();
                break;

            case NeedleState.Thrown:
                break;

            case NeedleState.Hit:
                break;

            default:
                break;
        }
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
        needleCollision.gameObject.transform.rotation = rotation;
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
        needleCollision.gameObject.transform.position = position;
    }
}
