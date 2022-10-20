using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneTrailFade : MonoBehaviour
{
    public void BreakOff()
    {
        transform.parent = null;
        Destroy(gameObject, 6.0f);
    }
}
