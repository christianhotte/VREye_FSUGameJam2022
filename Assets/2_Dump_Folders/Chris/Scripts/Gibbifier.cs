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
            rb.AddExplosionForce(explosionForce, explosionCenter.position, explosionRadius);
        }
    }
}
