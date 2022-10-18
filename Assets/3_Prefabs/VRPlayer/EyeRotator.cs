using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the beholder's eye light rotate smoothly based on head orientation.
/// </summary>
public class EyeRotator : MonoBehaviour
{
    //Objects & Components:
    [SerializeField(), Tooltip("Target position and orientation which eye will move to and is trying to rotate to")] private Transform target;

    //Settings:
    [Space()]
    [SerializeField(), Tooltip("Eye's positional offset from given target position")] private Vector3 offset;
    [SerializeField(), Tooltip("Rate at which eye lerps toward target")]              private float lerpRate;

    //RUNTIME METHODS:
    private void Update()
    {
        //Get target rotation:
        Quaternion newRotation = transform.rotation;                                            //Get current rotation
        newRotation = Quaternion.Lerp(newRotation, target.rotation, lerpRate * Time.deltaTime); //Lerp toward target rotation

        //Cleanup:
        transform.position = target.transform.position + offset; //Snap position to target (with offset)
        transform.rotation = newRotation;                        //Set new rotation
    }
}
