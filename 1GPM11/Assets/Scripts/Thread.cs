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
