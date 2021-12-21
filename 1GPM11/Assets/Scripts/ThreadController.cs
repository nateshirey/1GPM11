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

    public void AnchorThread(Vector3 position)
    {
        threadPool[threadIndex].AnchorFromNeedleToWall(position);
    }
}
