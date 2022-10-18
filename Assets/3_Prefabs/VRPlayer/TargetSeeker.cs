using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes an object seek toward target transform position.
/// </summary>
public class TargetSeeker : MonoBehaviour
{
    //Objects & Components:
    [SerializeField(), Tooltip("Target position which this system will seek toward")] private Transform target;
    private Rigidbody rb; //Rigidbody component this seeker is using to move toward target

    //Settings:
    [SerializeField(), Tooltip("Rate at which this object seeks target position")] private float linearFollowSpeed;
    [SerializeField(), Tooltip("Rate at which this object seeks target rotation")] private float angularFollowSpeed;

    //Runtime Vars:


    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        if (!TryGetComponent(out rb)) Debug.LogError("TargetSeeker on " + name + " is missing Rigidbody component!"); //Get rigidbody and post error if it is missing
    }
    private void Update()
    {
        //Update rigidbody:
        rb.MovePosition(Vector3.Lerp(transform.position, target.position, linearFollowSpeed * Time.deltaTime));      //Update position
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, target.rotation, angularFollowSpeed * Time.deltaTime)); //Update orientation
    }
}
