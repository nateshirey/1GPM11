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

    public void SetUpThread(Transform startTrans, Vector3 endTrans)
    {
        if(threadIndex > threadPool.Count - 1)
        {
            threadIndex = 0;
        }

        threadPool[threadIndex].SetAnchors(startTrans, endTrans);
    }
}
