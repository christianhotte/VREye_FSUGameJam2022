using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKiller : MonoBehaviour
{
    //Settings:
    [Header("Killer Settings:")]
    [SerializeField] private bool instaKill;
    [Min(0), SerializeField] private float shoveForce;
    [Min(0), SerializeField] private float shoveBounce;

    //Runtime Variables:
    [SerializeField] internal bool doSquish;

    //RUNTIME METHODS:
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FPSPlayer playerController)) //Hand has collided with a player
        {
            if (instaKill)
            {
                if (doSquish) playerController.Squish();
                else playerController.Die();
            }
            else
            {
                playerController.TakeDamage();
                Vector3 shoveDirection = (other.transform.position - transform.position).normalized;
                shoveDirection *= shoveForce;
                playerController.SendOutOfControl(new Vector2(shoveDirection.x, shoveDirection.z), shoveBounce);
            }
        }
    }
}
