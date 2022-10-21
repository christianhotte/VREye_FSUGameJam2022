using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gibbifier : MonoBehaviour
{
    public float explosionForce;
    public float explosionRadius;
    public Transform explosionCenter;

    private void Start()
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            Vector3 center = transform.position;
            if (explosionCenter != null) center = explosionCenter.position;
            rb.AddExplosionForce(explosionForce, center, explosionRadius);
        }
    }
}
