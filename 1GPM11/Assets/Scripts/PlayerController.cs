using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody rb;

    [Header("Player Movement Values")]
    [Range(1, 20)]
    public float acceleration = 10f;
    [Range(0, 0.99f)]
    public float decceleration = 0.95f;
    [Range(5, 10)]
    public float maxSpeed = 5;

    public float speed;

    public float inputValue;

    public void OnMove(InputValue value)
    {
        inputValue = value.Get<float>();
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
    }

    private void AddForces()
    {
        Vector3 inputForce = new Vector3(inputValue, 0, 0) * acceleration;
        speed = Mathf.Abs(rb.velocity.x);
        if(speed < maxSpeed)
        {
            rb.AddForce(inputForce, ForceMode.Force);
        }

        rb.velocity *= decceleration;
    }
}
