using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneCam : MonoBehaviour
{
    Camera cam;
    Animation anim;
    public AnimationClip closeClip;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        anim = GetComponent<Animation>();
    }

    private void Start()
    {
        StartCoroutine(WaitThenDestroy());
    }

    IEnumerator WaitThenDestroy()
    {
        yield return new WaitForSeconds(5.0f);
        anim.AddClip(closeClip, "Close");
        anim.Play("Close");
        yield return new WaitForSeconds(1.0f);
        Destroy(transform.parent.gameObject);
    }
}
