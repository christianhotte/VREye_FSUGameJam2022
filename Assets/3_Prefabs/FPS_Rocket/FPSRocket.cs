using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRocket : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float turnDownSpeed;
    [SerializeField] LayerMask canHit;
    [SerializeField] LayerMask destroyOnHit;
    [SerializeField] int monsterHeadLayer;
    [SerializeField] int monsterHandLayer;
    [SerializeField] ParticleSystem ps;
    [SerializeField] GameObject boltModel;

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

            IShootable shot = hit.collider.GetComponent<IShootable>();
            GetComponent<AudioSource>().Play();
            if (hit.collider.gameObject.layer == monsterHeadLayer || hit.collider.gameObject.layer == monsterHandLayer)
            {
                Destroy(boltModel);
                transform.parent = hit.collider.transform;
            }
            if (shot != null)
            {
                shot.Shot();
                Destroy(gameObject); //Make sure projectile just goes away when hitting a weak point
            }
            else
            {
                if (destroyOnHit == (destroyOnHit | (1 << hit.collider.gameObject.layer)))
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
            }
        }
        else
        {
            transform.Translate(Vector3.forward * distanceToMove);
            transform.RotateAround(transform.position, transform.right, turnDownSpeed * Time.deltaTime);
        }
    }
}
