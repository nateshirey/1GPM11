using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NeedleState { Held, Thrown, Hit }

public class NeedleCollision : MonoBehaviour
{
    private Rigidbody rb;
    public NeedleState needleState;
    public Transform needleTip;

    private Vector3 sewPosition = Vector3.zero;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        needleState = NeedleState.Held;
    }

    public void ThrowNeedle(float throwStrength)
    {
        needleState = NeedleState.Thrown;

        Vector3 throwVector = rb.gameObject.transform.right;
        rb.AddForce(throwVector * throwStrength, ForceMode.Impulse);
        Debug.Log("Throw");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.gameObject.CompareTag("SewableWall"))
        {
            sewPosition = other.ClosestPointOnBounds(needleTip.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(sewPosition, 0.5f);
    }
}
