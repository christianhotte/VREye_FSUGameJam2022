using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakSpot : MonoBehaviour, IShootable
{
    //Objects & Components:
    private AudioSource audioSource;

    //Settings:
    [Header("Settings:")]
    [Min(1), SerializeField, Tooltip("Damage dealt when hitting this weak spot")] private int damage = 1;
    [SerializeField, Tooltip("Sound made when this boss is hit")]                 private AudioClip sound;

    //RUNTIME METHODS:
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); //Get audio source component
    }
    public void Shot()
    {
        if (VRPlayerController.main != null) //VRPlayer exists in scene
        {
            print("Dealt " + damage + " damage!");
            VRPlayerController.DealDamage(damage); //Deal damage to VR player

            if (audioSource != null && sound != null) audioSource.PlayOneShot(sound); //Play hurt sound
        }
    }
}
