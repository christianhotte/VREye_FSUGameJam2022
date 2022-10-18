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
            //Quaternion oldRotation = transform.rotation;
            //Vector3 oldScale = transform.localScale;
            //transform.parent = hit.collider.transform;
            transform.position = hit.point;
            //transform.localScale = oldScale;
            //transform.rotation = oldRotation;
            IShootable shot = hit.collider.GetComponent<IShootable>();
            if (shot != null)
            {
                shot.Shot();
            }
            Destroy(this);
        }
        else
        {
            transform.Translate(Vector3.forward * distanceToMove);
        }
    }
}
