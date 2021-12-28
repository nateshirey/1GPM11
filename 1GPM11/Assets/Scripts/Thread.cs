using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thread : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField]
    private LineRenderer secondLineRenderer;

    private List<Segment> threadSegments = new List<Segment>();

    [Header("User Parameters")]
    [Range(0.2f, 5)]
    public float segmentLength = 0.25f;
    public int segmentsNumber = 15;
    [Range(0.2f, 1f)]
    public float width = 0.5f;
    [Range(1, 50)]
    public int iterations = 20;
    [Range(-1.5f, -10f)]
    public float gravity = -1.5f;
    public float offsetAmount;
    public LayerMask collisionMask;
    public Collider boxCollider;
    [Range(0f, 5f)]
    public float cutResetTime = 3f;

    [Header("Debug No Touch")]
    public Transform startTransform;
    private Vector3 startPosition;
    public Vector3 endPosition;
    private Vector3 anchorPosition;
    public bool anchoredToWall = false;
    private bool collided;
    private bool cut = false;
    private int collidedSegmentIndex;
    private int cutSegmentIndex;

    private void Awake()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.endWidth = width;
        lineRenderer.startWidth = width;

        secondLineRenderer.endWidth = width;
        secondLineRenderer.startWidth = width;

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

        if (cut)
        {
            Vector3[] line1Points = new Vector3[cutSegmentIndex];
            for (int i = 0; i < cutSegmentIndex; i++)
            {
                line1Points[i] = positions[i];
            }
            lineRenderer.positionCount = line1Points.Length;
            lineRenderer.SetPositions(line1Points);

            Vector3[] line2Points = new Vector3[threadSegments.Count - 1 - cutSegmentIndex];
            for (int i = 0; i < line2Points.Length; i++)
            {
                line2Points[i] = positions[positions.Length - 1 - i];
            }
            secondLineRenderer.positionCount = line2Points.Length;
            secondLineRenderer.SetPositions(line2Points);
        }

        else
        {
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
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

        //if the thread is anchored to the wall and another point we can
        //now collide with it and cut it. The player should not be able
        //to do this otherwise.
        if (anchoredToWall)
        {
            startPosition = anchorPosition;

            collided = false;
            SetCollisionActive(false);

            if (!cut)
            {
                for (int i = 0; i < segmentsNumber - 1; i++)
                {
                    if (collided)
                    {
                        break;
                    }
                    else
                    {
                        collided = CheckCollisions(i);
                        collidedSegmentIndex = i;
                    }
                }

                if (boxCollider.isTrigger && !collided)
                {
                    boxCollider.isTrigger = false;
                }

                for (int i = 0; i < iterations; i++)
                {
                    ApplyConstraints();
                }
            }

            else
            {
                for (int i = 0; i < iterations; i++)
                {
                    ApplyConstraintsCut();
                }
            }
        }

        else
        {
            SetCollisionActive(false);

            for (int i = 0; i < iterations; i++)
            {
                ApplyConstraints();
            }
        }
    }

    private void Constrain(int i)
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
        if (i > 0)
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
            Constrain(i);
        }

    }

    private void ApplyConstraintsCut()
    {
        Segment firstSegment = threadSegments[0];
        firstSegment.posNow = startPosition;
        threadSegments[0] = firstSegment;

        Segment endSegment = threadSegments[threadSegments.Count - 1];
        endSegment.posNow = endPosition;
        threadSegments[threadSegments.Count - 1] = endSegment;

        for (int i = 0; i < cutSegmentIndex - 1; i++)
        {
            Constrain(i);
        }

        for (int i = threadSegments.Count - 2; i > cutSegmentIndex; i--)
        {
            Constrain(i);
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

            Vector3 tangent = threadSegments[0].posNow - threadSegments[threadSegments.Count - 1].posNow;

            Vector3 colliderPos = collidedBody.position;
            Vector3 endPos = threadSegments[threadSegments.Count - 1].posNow;
            Vector3 relativePos = collidedBody.position - endPos;
            Vector3 p = Vector3.Project(relativePos, tangent);
            p.y += endPos.y;
            colliderPos.y = p.y;

            tangent.Normalize();
            Vector3 normal = Vector3.Cross(tangent, Vector3.forward);
            normal.Normalize();
            Quaternion rotation = Quaternion.LookRotation(tangent, normal);

            float offset = Mathf.Pow(Mathf.Abs(Vector3.Dot(tangent, Vector3.up)), 3);
            colliderPos.y -= offset * offsetAmount;

            boxCollider.transform.SetPositionAndRotation(colliderPos, rotation);

            if (anchoredToWall && footHeight >= colliderPos.y)
            {
                SetCollisionActive(true);

                float d = Vector3.Distance(colliderPos, nextSegment.posNow);
                float ratio = d / segmentLength;
                Vector3 dir = (collidedBody.transform.position - colliderPos).normalized;
                nextSegment.posNow += (Vector2)(dir * gravity * Time.fixedDeltaTime * 20 * ratio);
                threadSegments[startIndex + 1] = nextSegment;
                currentSegment.posNow += (Vector2)(dir * gravity * Time.fixedDeltaTime * 20 * (1 - ratio));
                threadSegments[startIndex] = currentSegment;
            }

            else
            {
                SetCollisionActive(false);
            }

            return true;
        }

        return false;
    }

    private void SetCollisionActive(bool active)
    {
        boxCollider.gameObject.SetActive(active);
    }

    public void DropThrough()
    {
        boxCollider.isTrigger = true;
    }

    public void CutThread()
    {
        if (collided && anchoredToWall && !cut)
        {
            cutSegmentIndex = collidedSegmentIndex;
            cut = true;
            secondLineRenderer.gameObject.SetActive(true);
            StartCoroutine(CutResetCoroutine());
        }
    }

    private IEnumerator CutResetCoroutine()
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        float alpha = 1;
        float timer = 0f;
        while(timer < cutResetTime)
        {
            timer += Time.deltaTime;
            alpha = (cutResetTime - timer) / cutResetTime;
            Color newColor = propertyBlock.GetColor("_Color");
            newColor.a = alpha;
            propertyBlock.SetColor("_Color", newColor);
            lineRenderer.SetPropertyBlock(propertyBlock);
            secondLineRenderer.SetPropertyBlock(propertyBlock);
            yield return null;
        }
        cut = false;

        secondLineRenderer.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
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
