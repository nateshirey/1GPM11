using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody rb;
    public Transform groundTransform;
    public GameEvent dropThroughEvent;
    public GameEvent cutEvent;

    [Header("Player Movement Values")]
    [Range(5, 50)]
    public float acceleration = 10f;
    [Range(0, 0.99f)]
    public float decceleration = 0.95f;
    [Range(5, 10)]
    public float maxSpeed = 5;
    [Range(5, 10)]
    public float jumpForce = 5f;
    [Range(0.01f, 1f)]
    public float groundCheckDistance = 0.5f;

    [Header("Debug")]
    //these are public to view the values in inspector but should go private
    public bool grounded;

    public float speed;

    public float inputValue;

    //a few public methods here to read input values from the player input component into here
    public void OnMove(InputValue value)
    {
        inputValue = value.Get<float>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed && grounded)
        {
            rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }

    public void OnDropThrough(InputValue value)
    {
        if (value.isPressed)
        {
            dropThroughEvent.Raise();
        }
    }

    public void OnCut(InputValue value)
    {
        if (value.isPressed)
        {
            cutEvent.Raise();
        }
    }

    private void Awake()
    {
        if(rb == null)
        {
            rb = this.GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        AddForces();
        GroundCheck();
    }

    //using the input values we have, add forces to the player to move
    private void AddForces()
    {
        Vector3 inputForce = new Vector3(inputValue, 0, 0) * acceleration;
        if (!grounded)
        {
            inputForce *= 0.5f;
        }

        speed = Mathf.Abs(rb.velocity.x);
        if(speed < maxSpeed)
        {
            rb.AddForce(inputForce, ForceMode.Force);
        }

        Vector3 velocity = rb.velocity;
        velocity.x *= decceleration;
        rb.velocity = velocity;
    }

    private void GroundCheck()
    {
        grounded = false;

        Ray ray = new Ray(groundTransform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, groundCheckDistance)){
            grounded = true;
        }
    }
}
