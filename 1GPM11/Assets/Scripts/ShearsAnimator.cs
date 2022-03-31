using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShearsAnimator : MonoBehaviour
{
    public Animator anim;
    public Transform foot;

    public void Cut()
    {
        anim.gameObject.transform.position = foot.position;
        anim.SetTrigger("Cut");
    }
}
