using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPlayerController : MonoBehaviour
{
    //Static Stuff:
    /// <summary>
    /// Single instance of this script in scene.
    /// </summary>
    public static VRPlayerController main;

    //Objects & Components:
    private Transform leftHand;  //Position and orientation of left hand in scene
    private Transform rightHand; //Position and orientation of right hand in scene

    //Settings:
    [Header("General Settings:")]
    [SerializeField(), Tooltip("Health the player starts at")] private int maxHealth;

    //Runtime Vars:
    private int health; //Amount of health player currently has

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (main == null) { main = this; } else { Destroy(gameObject); } //Destroy this instance of player if it is duplicated in scene
        health = maxHealth;                                              //Set initial health
    }

    //UTILITY METHODS:
    public void SendHapticImpulse()
    {

    }
}
