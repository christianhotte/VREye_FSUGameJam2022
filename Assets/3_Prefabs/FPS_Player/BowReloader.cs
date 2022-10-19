using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowReloader : MonoBehaviour
{
    [SerializeField] FPSPlayer player;

    public void ReloadBow()
    {
        player.ReloadBow();
    }
}
