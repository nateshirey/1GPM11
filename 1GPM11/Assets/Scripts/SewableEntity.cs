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
