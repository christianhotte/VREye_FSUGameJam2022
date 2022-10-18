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

    //Settings:
    [SerializeField(), Tooltip("Rate at which this object seeks target position")] private float linearFollowSpeed;
    [SerializeField(), Tooltip("Rate at which this object seeks target rotation")] private float angularFollowSpeed;

    //Runtime Vars:
    private Vector3 velocity; //Last recorded linear velocity of this seeker object

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        
    }
    private void Update()
    {
        DoSeeking(Time.deltaTime); //Seek on update
    }

    //FUNCTIONALITY METHODS:
    private void DoSeeking(float deltaTime)
    {
        //Validity checks:
        if (target == null) return; //Ignore if target is null

        //Update position:
        Vector3 newPosition = transform.position; //Get current position as modifiable variable
        newPosition = Vector3.Lerp(transform.position, target.position, linearFollowSpeed * Time.deltaTime);

        velocity = newPosition - transform.position; //Record current velocity
        transform.position = newPosition;            //Set new position

        //Update rotation:
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, angularFollowSpeed * Time.deltaTime);
    }

    //OPERATION METHODS:
    /// <summary>
    /// Sets new target for seeker system.
    /// </summary>
    public void SetTarget(Transform newTarget) { target = newTarget; }
}
