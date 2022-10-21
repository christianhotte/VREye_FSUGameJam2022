using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDoor : MonoBehaviour
{
    public void DoShake()
    {
        FPSPlayer.FPSShake(0.05f, 6, 0.2f, 0.05f);
        BossHpBar.DisplayBossHealth();
    }
}
