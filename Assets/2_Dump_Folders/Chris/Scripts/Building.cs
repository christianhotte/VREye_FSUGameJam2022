using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    //Objects & Components:
    private Transform model;         //Model transform for this building
    private AudioSource audioSource; //Audiosource component

    //Settings:
    [Header("Shake Settings:")]
    [SerializeField, Tooltip("Maximum speed at which house can shake")]                                                             private float maxShakeFreq;
    [SerializeField, Tooltip("Maximum distance by which house shakes")]                                                             private float maxShakeDist;
    [SerializeField, Tooltip("Modifies shake perpendicularly to cross-axis position in order to make it appear a bit more random")] private float shakeAxisOffset;
    [SerializeField, Tooltip("Curve describing intensity of shake based on net pull force")]                                        private AnimationCurve shakeIntensityCurve;
    [Header("Pull Settings:")]
    [SerializeField, Tooltip("Net pull force at which building will come loose from the ground")] private float tearForce;
    [Header("Sounds:")]
    [SerializeField] private AudioClip strainSound;
    [SerializeField] private AudioClip pullFreeSound;

    //Runtime vars:
    private List<VRHandController> grabbingHands = new List<VRHandController>(); //Player hands currently grabbing this building

    internal bool uprooted = false; //Turns true when building is torn out of the ground
    private float netPullForce;     //Total amount of force dedicated to pulling on this building
    private float currentShakeFrequency;
    private float currentShakeMagnitude;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        model = GetComponentInChildren<MeshFilter>().transform; //Get model transform
        audioSource = GetComponent<AudioSource>();              //Get audiosource
    }
    private void Update()
    {
        if (!uprooted) //Building is not currently uprooted
        {
            
        }
    }

    //OPERATION METHODS:
    /// <summary>
    /// Grabs this building with given hand.
    /// </summary>
    public void Grab(VRHandController hand)
    {
        //Initialize:
        if (!grabbingHands.Contains(hand)) grabbingHands.Add(hand);
        if (grabbingHands.Count > 1) //Building is being grabbed with both hands
        {
            audioSource.PlayOneShot(strainSound);
        }
    }
    /// <summary>
    /// Pull on grabbed building.
    /// </summary>
    public void Pull(Vector3 force)
    {
        netPullForce = Mathf.Max(0, netPullForce + force.y); //Add force to net pull force (do not go below zero)
        if (netPullForce >= tearForce)
        {
            //Tear up building:
            uprooted = true; //Indicate that building is uprooted
        }
    }
    /// <summary>
    /// Indicates that this building has been released by given hand.
    /// </summary>
    public void Release(VRHandController hand)
    {
        //Cleanup:
        if (grabbingHands.Contains(hand)) grabbingHands.Remove(hand);
        netPullForce = 0;
    }
}
