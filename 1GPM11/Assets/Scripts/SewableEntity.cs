using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SewableEntity : MonoBehaviour
{
    public bool anchored = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    //this is called from needle collision after a needle has collided with this object
    //we need to remove the rigidbody from the simulation and start moving it
    public void Sew(Vector3 SewPosition, float sewSpeed)
    {
        anchored = true;
        rb.useGravity = false;
        StartCoroutine(SewCoroutine(SewPosition, sewSpeed));
    }

    private void OnCollisionEnter(Collision collision)
    {
        //maybe change this to a layer mask
        if (collision.gameObject.CompareTag("SewableWall") && anchored)
        {
            StopAllCoroutines();
            rb.isKinematic = true;
        }
    }
    //This coroutine moves the entity to the desired location,
    //the speed is passed from the needle controller so that the entity moves
    //at the same rate as the needle as it is pulled back to the player
    private IEnumerator SewCoroutine(Vector3 SewPosition, float speed)
    {
        while(this.transform.position != SewPosition)
        {
            Vector3 nextPosition = Vector3.MoveTowards(transform.position,
                                                       SewPosition,
                                                       Time.deltaTime * speed);


            transform.position = nextPosition;
            yield return null;
        }

        yield break;
    }
}
