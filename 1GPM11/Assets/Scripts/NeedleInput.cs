using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class NeedleInput : MonoBehaviour
{
    [Header("Component References")]
    public NeedleCollision needleCollision;
    private Camera cam;

    [Header("Editable Parameters")]
    [Range(0.5f, 10f)]
    public float throwStrength = 1f;
    [Range(1f, 40f)]
    public float returnSpeed = 1f;
    [Range(1f, 5f)]
    public float maxPullbackMagnitude = 1f;
    [Range(0.1f, 0.5f)]
    public float mouseNormalizeRadius = 0.2f;


    private Vector2 inputValue;
    private Vector2 mousePos;

    private bool pulledBack = false;
    private bool canThrow = false;

    private float pullbackAmount = 0;
    private float previousPullbackAmount = 0;
    private float throwAngle;
    private float mouseNormalRatio;


    private void Awake()
    {
        cam = Camera.main;
        mouseNormalRatio = 1f / mouseNormalizeRadius;
    }

    //only used for controller input
    public void OnPullback(InputValue value)
    {
        if(needleCollision.needleState == NeedleState.Held)
        {
            inputValue = value.Get<Vector2>();
            pulledBack = inputValue.magnitude > 0 ? true : false;
        }
    }

    //used in both schemes
    public void OnRetrieve(InputValue value)
    {
        if (!value.isPressed && needleCollision.needleState != NeedleState.Held)
        {
            needleCollision.StartReturn();
        }
    }

    //only used for mouse/keyboard input
    public void OnMousePosition(InputValue value)
    {
        if(needleCollision.needleState == NeedleState.Held)
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
    }

    public void OnMouseClick(InputValue value)
    {
        if(needleCollision.needleState == NeedleState.Held)
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
                needleCollision.ThrowNeedle(throwStrength, this.transform.position, returnSpeed);
            }
        }
    }

    private void FixedUpdate()
    {
        switch (needleCollision.needleState)
        {
            case NeedleState.Held:
                PointNeedle();
                PositionNeedleAtPlayer();
                break;

            case NeedleState.Thrown:
                break;

            case NeedleState.HitWall:
                break;

            case NeedleState.Returning:
                ReturnNeedle();
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

    private void PositionNeedleAtPlayer()
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

    private void ReturnNeedle()
    {
        Transform needleTransform = needleCollision.transform;
        Vector3 nextPosition = Vector3.MoveTowards(needleTransform.position, this.transform.position, returnSpeed * Time.fixedDeltaTime);
        needleCollision.transform.position = nextPosition;

        Vector3 normalizedPos = needleTransform.position - this.transform.position;
        float returnAngle = Mathf.Atan2(normalizedPos.y, normalizedPos.x) * Mathf.Rad2Deg - 180f;
        needleCollision.transform.rotation = Quaternion.Euler(0, 0, returnAngle);

        float dist = Mathf.Abs((this.transform.position - nextPosition).sqrMagnitude);

        if(dist < 0.1f)
        {
            needleCollision.needleState = NeedleState.Held;
        }
    }
}
