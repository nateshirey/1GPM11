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

    public void Sew(Vector3 SewPosition)
    {
        anchored = true;
        rb.useGravity = false;
        StartCoroutine(SewCoroutine(SewPosition));
    }

    private IEnumerator SewCoroutine(Vector3 SewPosition)
    {
        while(this.transform.position != SewPosition)
        {
            Vector3 nextPosition = Vector3.MoveTowards(transform.position,
                                                       SewPosition,
                                                       Time.deltaTime * 5f);
            transform.position = nextPosition;
            yield return null;
        }

        yield break;
    }
}
