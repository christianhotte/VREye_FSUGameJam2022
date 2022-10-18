using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSRocket : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    private void Start()
    {
        Destroy(gameObject, 2.0f);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        moveSpeed += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
