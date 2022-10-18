using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRocket : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float turnDownSpeed;
    [SerializeField] LayerMask canHit;

    private void Start()
    {
        Destroy(gameObject, 5.0f);
    }

    private void Update()
    {
        float distanceToMove = Time.deltaTime * moveSpeed;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanceToMove, canHit))
        {
            transform.position = hit.point;
            Destroy(this);
        }
        else
        {
            transform.Translate(Vector3.forward * distanceToMove);
        }
    }
}
