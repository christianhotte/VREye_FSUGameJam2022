using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRocket : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] LayerMask canHit;

    private void Start()
    {
        Destroy(gameObject, 2.0f);
    }

    private void Update()
    {
        float distanceToMove = Time.deltaTime * moveSpeed;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanceToMove, canHit))
        {
            Destroy(gameObject);
        }
        else
        {
            transform.Translate(Vector3.forward * distanceToMove);
            moveSpeed += moveSpeed * Time.deltaTime * 10.0f;
            if (moveSpeed > 50.0f) moveSpeed = 50.0f;
        }
    }
}
