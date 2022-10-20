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
    [SerializeField, Tooltip("Maximum speed at which house can shake")]                      private float maxShakeFreq;
    [SerializeField, Tooltip("Maximum distance by which house shakes")]                      private float maxShakeDist;
    [SerializeField, Tooltip("Curve describing intensity of shake based on net pull force")] private AnimationCurve shakeIntensityCurve;
    [Header("Pull Settings:")]
    [SerializeField, Tooltip("Net pull force at which building will come loose from the ground")] private float tearForce;
    [Range(0, 1), SerializeField]                                                                 private float strainForceValue;
    [Header("Sounds:")]
    [SerializeField] private AudioClip strainSound;
    [SerializeField] private AudioClip pullFreeSound;

    //Runtime vars:
    private List<VRHandController> grabbingHands = new List<VRHandController>(); //Player hands currently grabbing this building

    internal bool uprooted = false; //Turns true when building is torn out of the ground
    private float netPullForce;     //Total amount of force dedicated to pulling on this building
    private bool strainTriggered;
    private Vector3 startingPos;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        model = GetComponentInChildren<MeshFilter>().transform; //Get model transform
        audioSource = GetComponent<AudioSource>();              //Get audiosource
        startingPos = model.localPosition;
    }
    private void Update()
    {
        if (!uprooted && grabbingHands.Count > 1) //Building is not currently uprooted but is grabbed with both hands
        {
            if (netPullForce > tearForce * strainForceValue)
            {
                float speedValue = maxShakeFreq;
                float amountValue = maxShakeDist * shakeIntensityCurve.Evaluate(netPullForce / tearForce);

                Vector3 newPosition = model.localPosition;
                newPosition.x = startingPos.x + Mathf.Sin(Time.time * speedValue) * amountValue;
                newPosition.z = startingPos.z + Mathf.Sin(Time.time * speedValue) * amountValue;

                model.localPosition = newPosition;
            }
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
            model.GetComponent<MeshCollider>().enabled = false;
        }
    }
    /// <summary>
    /// Pull on grabbed building.
    /// </summary>
    public void Pull(Vector3 force)
    {
        if (!uprooted)
        {
            print("TotalForce: " + netPullForce);
            netPullForce = Mathf.Max(0, netPullForce + force.y);        //Add force to net pull force (do not go below zero)
            float forceValue = Mathf.Clamp01(netPullForce / tearForce); //Get value representing how pulled building is

            if (!strainTriggered && forceValue > strainForceValue)
            {
                strainTriggered = true;
                audioSource.PlayOneShot(strainSound);
            }
            if (netPullForce >= tearForce)
            {
                //Tear up building:
                uprooted = true; //Indicate that building is uprooted
                model.localPosition = startingPos; //Return model to normal position
                audioSource.PlayOneShot(pullFreeSound); //Play pull sound
            }
            if (netPullForce <= 0)
            {
                strainTriggered = false;
                model.localPosition = startingPos; //Return model to normal position
            }
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
        strainTriggered = false;
        model.localPosition = startingPos; //Return model to normal position
        model.GetComponent<MeshCollider>().enabled = true;
    }
}
