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

    public bool grounded;

    public float speed;

    public float inputValue;

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

        if (Physics.Raycast(ray, out hit, 1f)){
            if(hit.distance < groundCheckDistance)
            {
                grounded = true;
            }
        }
    }
}
