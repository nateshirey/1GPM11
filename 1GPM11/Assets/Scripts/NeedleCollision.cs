using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NeedleState { Held, Thrown, HitWall, Returning }

//This class controls the needle as it interacts with physics and the needle state
public class NeedleCollision : MonoBehaviour
{
    private Rigidbody rb;
    public NeedleState needleState;
    public Transform needleTip;
    public Transform eyeHole;

    private ThreadController threadController;

    private Vector3 sewPosition = Vector3.zero;
    private float sewSpeed = 5f;

    bool firstThread;

    private List<SewableEntity> hitEntities = new List<SewableEntity>();

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        needleState = NeedleState.Held;
        hitEntities.Clear();

        if(this.TryGetComponent<ThreadController>(out ThreadController tController))
        {
            threadController = tController;
        }

        firstThread = true;
    }

    //This method is called from the NeedleInput script.
    // 1) change the state
    // 2) start the thread controller to follow the needle
    // 3) apply force
    public void ThrowNeedle(float throwStrength, Vector3 anchorPos, float newSewSpeed)
    {
        // 1)
        needleState = NeedleState.Thrown;
        rb.isKinematic = false;

        sewSpeed = newSewSpeed;

        //special behaviour if we have not thrown the needle yet
        // 2)
        if(firstThread && threadController != null)
        {
            firstThread = false;
            threadController.DispatchThread(eyeHole, anchorPos);
        }
        // 3)
        Vector3 throwVector = rb.gameObject.transform.right;
        rb.AddForce(throwVector * throwStrength, ForceMode.Impulse);
    }

    // 1) make the needle stop responding to physics
    // 2) sew enemies to the wall and setup a new thread
    // 3) set new state
    public void StartReturn()
    {
        // 1)
        StopNeedle();
        // 2)
        if(needleState == NeedleState.HitWall)
        {
            SewEntities();
            if(threadController != null)
            {
                threadController.AnchorThread(needleTip.position);
                threadController.DispatchThread(eyeHole, needleTip.position);
            }
        }
        // 3)
        needleState = NeedleState.Returning;
    }

    // if the needle touches a trigger that is sewable
    private void OnTriggerEnter(Collider other)
    {
        if(needleState == NeedleState.Thrown)
        {
            //if its a wall mark the hit point for the thread
            if (other.gameObject.CompareTag("SewableWall"))
            {
                sewPosition = other.ClosestPointOnBounds(needleTip.position);
                needleState = NeedleState.HitWall;
                StopNeedle();
            }
            // if its an enemy add them to the list of sewn enemies
            else if (other.gameObject.CompareTag("SewableEntity"))
            {
                if(other.TryGetComponent<SewableEntity>(out SewableEntity entity))
                {
                    hitEntities.Add(entity);
                }
            }
        }
    }

    private void StopNeedle()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
    }

    //when we want to pull the needle back we call this to attach the entity to a sewable wall
    private void SewEntities()
    {
        foreach (SewableEntity e in hitEntities)
        {
            e.Sew(sewPosition, sewSpeed);
        }

        hitEntities.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(sewPosition, 0.5f);
    }
}
