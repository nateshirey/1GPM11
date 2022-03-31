using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadController : MonoBehaviour
{
    public List<Thread> threadPool;
    private int threadIndex;

    private void Awake()
    {
        threadIndex = 0;
    }

    //This method starts simulating the next thread and sets up the end points as transforms so the update ass objects move
    public void DispatchThread(Transform startTrans, Vector3 endTrans)
    {
        threadIndex++;

        if (threadIndex > threadPool.Count - 1)
        {
            threadIndex = 0;
        }

        threadPool[threadIndex].gameObject.SetActive(true);
        threadPool[threadIndex].SetAnchors(startTrans, endTrans);
    }

    //this method is called from the needle collision script when the needle is called back to the player
    //and causes the thread controller to change the endpoint from the needle eyehole to a hit point on the wall
    public void AnchorThread(Vector3 position)
    {
        threadPool[threadIndex].AnchorFromNeedleToWall(position);
    }
}
