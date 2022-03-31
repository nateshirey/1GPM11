using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class controls a single instance of a thread
//the basics on solving the positions of the thread segments has been taken from Jasony on youtube
//the collisions and cutting the rope have been added by me. I have also made some general changes
//to the simulation part as I needed for the collision to work
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
        //line setup
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.endWidth = width;
        lineRenderer.startWidth = width;

        //setup also but this won't be used until the thread it segmented
        secondLineRenderer.endWidth = width;
        secondLineRenderer.startWidth = width;

        Vector3 startPoint = startTransform.position;

        //this creates a new thread
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

    //this method is set by the thread controller to move the end point around
    public void SetAnchors(Transform start, Vector3 end)
    {
        anchoredToWall = false;
        startTransform = start;
        endPosition = end;
    }

    //Also called from the thread controller when this thread is no longer the active thread attached to the needle
    public void AnchorFromNeedleToWall(Vector3 position)
    {
        anchoredToWall = true;
        anchorPosition = position;
    }

    private void Update()
    {
        DrawThread();
    }

    // 1) This method sets the line renderer position if not cut (Jasony)
    // 2) or set the positions on 2 renderers if cut (Me)
    private void DrawThread()
    {
        // 1)
        Vector3[] positions = new Vector3[segmentsNumber];
        for (int i = 0; i < segmentsNumber; i++)
        {
            positions[i] = threadSegments[i].posNow;
        }
        // 2)
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

    // this method moves the segment locations based on verlet integration
    private void Simulate()
    {
        //this part was largely provided by Jasony
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

        //this part was my addition
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
                //for each segment we need to check if the player should collide with it
                for (int i = 0; i < segmentsNumber - 1; i++)
                {
                    if (collided)
                    {
                        break;
                    }
                    else
                    {
                        collided = CheckCollisions(i);
                        //this tells us where to cut
                        collidedSegmentIndex = i;
                    }
                }

                if (boxCollider.isTrigger && !collided)
                {
                    boxCollider.isTrigger = false;
                }

                //this smooths out the positions of the segments iterations times
                for (int i = 0; i < iterations; i++)
                {
                    ApplyConstraints();
                }
            }

            else
            {
                //this also smooths out the segment positions but does it slightly
                //differently so that the cut point can move away from each other
                for (int i = 0; i < iterations; i++)
                {
                    ApplyConstraintsCut();
                }
            }
        }

        else
        {
            SetCollisionActive(false);
            //again just smoothing
            for (int i = 0; i < iterations; i++)
            {
                ApplyConstraints();
            }
        }
    }

    //this part was largely taken from Jasony
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
        //this is the end segment
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

    //this method does an overlap capsule to see if the player is touching a segment of the thread
    private bool CheckCollisions(int startIndex)
    {
        Segment currentSegment = threadSegments[startIndex];
        Segment nextSegment = threadSegments[startIndex + 1];

        //check if we touched the player at this segment
        Collider[] collisions = Physics.OverlapCapsule(currentSegment.posNow, nextSegment.posNow, width, collisionMask);
        if (collisions.Length > 0)
        {
            //if we did, assume we are pushing back the first contacted body
            Rigidbody collidedBody = collisions[0].attachedRigidbody;
            float footHeight = collisions[0].bounds.min.y;

            //this section ended up being a little over complicated but it creates a rotation and position
            //for the box colider on the thread so that is can try and stay ahead of the player while pushing them in th right direction
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

            //this is here to exagerate the movement if the thread is pointing more vertically.
            float offset = Mathf.Pow(Mathf.Abs(Vector3.Dot(tangent, Vector3.up)), 3);
            colliderPos.y -= offset * offsetAmount;

            boxCollider.transform.SetPositionAndRotation(colliderPos, rotation);

            //make sure the thread is under the player and attached to the wall at both ends
            if (anchoredToWall && footHeight >= colliderPos.y)
            {
                SetCollisionActive(true);

                //this section adds a slight woggle to the thread if the player is standing on it
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

    //these next 2 methods are called from game events when the player interacts with the thread
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

    //this coroutine fades the color of the thread
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
