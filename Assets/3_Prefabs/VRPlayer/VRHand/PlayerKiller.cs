using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKiller : MonoBehaviour
{
    //Settings:


    //RUNTIME METHODS:
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FPSPlayer playerController)) //Hand has collided with a player
        {
            playerController.Die(); //Kill player
            print("Killed Player");
        }
    }
}
