using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bell : MonoBehaviour, IShootable
{
    UnityEvent bellShotEvent = new UnityEvent();
    bool shot = false;

    public void Shot()
    {
        if (shot) return;
        GetComponent<AudioSource>().Play();
        shot = true;
        bellShotEvent.Invoke();
        FPSPlayer.FPSShake(0.1f, 10, 0.25f, 0.05f);
        BossHpBar.DisplayBossHealth();
    }
}
