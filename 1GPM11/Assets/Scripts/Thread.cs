using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thread : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private List<Segment> threadSegments = new List<Segment>();
    private float segmentLength = 0.25f;
    public int segmentsNumber = 15;
    [Range(0.2f, 1f)]
    public float width = 0.5f;
    [Range(1, 50)]
    public int iterations = 20;
    [Range(-1.5f, -10f)]
    public float gravity = -1.5f;

    public Transform startTransform;
    private Vector3 startPosition;
    public Vector3 endPosition;
    private Vector3 anchorPosition;
    public bool anchoredToWall = false;

    public LayerMask collisionMask;

    public BoxCollider boxCollider;

    private void Awake()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.endWidth = width;
        lineRenderer.startWidth = width;

        Vector3 startPoint = startTransform.position;

        for (int i = 0; i < segmentsNumber; i++)
        {
            threadSegments.Add(new Segment(startPoint));
            startPoint.y -= segmentLength;
        }

        if(boxCollider == null)
        {
            boxCollider = GetComponentInChildren<BoxCollider>();
        }
    }

    public void SetAnchors(Transform start, Vector3 end)
    {
        anchoredToWall = false;
        startTransform = start;
        endPosition = end;
    }

    public void AnchorFromNeedleToWall(Vector3 position)
    {
        anchoredToWall = true;
        anchorPosition = position;
    }

    private void Update()
    {
        DrawThread();
    }

    private void DrawThread()
    {
        Vector3[] positions = new Vector3[segmentsNumber];
        for (int i = 0; i < segmentsNumber; i++)
        {
            positions[i] = threadSegments[i].posNow;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        Vector2 gravityForce = new Vector2(0, gravity);

        for (int i = 0; i < segmentsNumber; i++)
        {
            Segment currentSegment = this.threadSegments[i];
            Vector2 velocity = currentSegment.posNow - currentSegment.posOld;
            currentSegment.posOld = currentSegment.posNow;
            currentSegment.posNow += velocity;
            currentSegment.posNow += gravityForce * Time.fixedDeltaTime;

            threadSegments[i] = currentSegment;
        }

        startPosition = startTransform.position;
        if (anchoredToWall)
        {
            startPosition = anchorPosition;
        }

        for (int i = 0; i < iterations; i++)
        {
            ApplyConstraints();
        }


        bool collided = false;
        for (int i = 0; i < segmentsNumber - 1; i++)
        {
            if (collided)
            {
                break;
            }
            else
            {
                collided = CheckCollisions(i);
            }
        }
    }

    private void ApplyConstraints()
    {
        Segment firstSegment = threadSegments[0];
        firstSegment.posNow = startPosition;
        threadSegments[0] = firstSegment;

        Segment endSegment = threadSegments[threadSegments.Count - 1];
        endSegment.posNow = endPosition;
        threadSegments[threadSegments.Count - 1] = endSegment;

        for (int i = 0; i < segmentsNumber - 1; i++)
        {
            Segment currentSegment = threadSegments[i];
            Segment nextSegment = threadSegments[i + 1];

            Vector2 moveDirection = currentSegment.posNow - nextSegment.posNow;
            float dist = moveDirection.magnitude;
            float sign = Mathf.Sign(dist - segmentLength);

            moveDirection *= sign;
            moveDirection.Normalize();

            //error is the difference in the current segment distance and the desired distance
            float error = Mathf.Abs(dist - segmentLength);

            Vector2 moveAmount = moveDirection * error;
            if(i > 0)
            {
                currentSegment.posNow -= moveAmount * 0.5f;
                threadSegments[i] = currentSegment;
                nextSegment.posNow += moveAmount * 0.5f;
                threadSegments[i + 1] = nextSegment;
            }
            else
            {
                nextSegment.posNow += moveAmount;
                threadSegments[i + 1] = nextSegment;
            }
        }
    }

    private bool CheckCollisions(int startIndex)
    {
        Segment currentSegment = threadSegments[startIndex];
        Segment nextSegment = threadSegments[startIndex + 1];

        Collider[] collisions = Physics.OverlapCapsule(currentSegment.posNow, nextSegment.posNow, width, collisionMask);
        if (collisions.Length > 0)
        {
            Rigidbody collidedBody = collisions[0].attachedRigidbody;
            float footHeight = collisions[0].bounds.min.y;

            Vector3 tangent = (currentSegment.posNow - nextSegment.posNow).normalized;
            Vector3 normal = Vector3.Cross(tangent, Vector3.forward);
            normal.Normalize();


            Vector3 colliderPos = collidedBody.position;
            float dist = Mathf.Abs(currentSegment.posNow.x - nextSegment.posNow.x);
            float t = (currentSegment.posNow.x - colliderPos.x) / dist;
            colliderPos.y = Mathf.Lerp(nextSegment.posNow.y, currentSegment.posNow.y, t) - ( width);
            //colliderPos += normal;

            if (anchoredToWall && footHeight >= colliderPos.y)
            {
                SetCollisionActive(true);
            }

            else
            {
                SetCollisionActive(false);
            }

            Quaternion rotation = Quaternion.LookRotation(tangent, normal);

            boxCollider.transform.position = colliderPos;
            boxCollider.transform.rotation = rotation;

            return true;
        }

        return false;
    }

    private void SetCollisionActive(bool active)
    {
        boxCollider.gameObject.SetActive(active);
    }

    public struct Segment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public Segment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }

}
