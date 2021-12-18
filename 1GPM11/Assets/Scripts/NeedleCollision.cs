using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NeedleState { Held, Thrown, HitWall, Returning }

public class NeedleCollision : MonoBehaviour
{
    private Rigidbody rb;
    public NeedleState needleState;
    public Transform needleTip;

    private Vector3 sewPosition = Vector3.zero;

    private List<SewableEntity> hitEntities = new List<SewableEntity>();

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        needleState = NeedleState.Held;
        hitEntities.Clear();
    }

    public void ThrowNeedle(float throwStrength)
    {
        needleState = NeedleState.Thrown;
        rb.isKinematic = false;

        Vector3 throwVector = rb.gameObject.transform.right;
        rb.AddForce(throwVector * throwStrength, ForceMode.Impulse);
        Debug.Log("Throw");
    }

    public void StartReturn()
    {
        StopNeedle();
        if(needleState == NeedleState.HitWall)
        {
            SewEntities();
        }
        needleState = NeedleState.Returning;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(needleState == NeedleState.Thrown)
        {
            Debug.Log(other.name);
            if (other.gameObject.CompareTag("SewableWall"))
            {
                sewPosition = other.ClosestPointOnBounds(needleTip.position);
                needleState = NeedleState.HitWall;
                StopNeedle();
            }
            else if (other.gameObject.CompareTag("SewableEntity"))
            {
                if(other.TryGetComponent<SewableEntity>(out SewableEntity entity))
                {
                    hitEntities.Add(entity);
                }
            }
            else
            {
                //i think stuff here happens on non sewable walls only ? 
            }
        }
    }

    private void StopNeedle()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
    }

    private void SewEntities()
    {
        foreach (SewableEntity e in hitEntities)
        {
            e.Sew(sewPosition);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(sewPosition, 0.5f);
    }
}
