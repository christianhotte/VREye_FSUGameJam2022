using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bell : MonoBehaviour, IShootable
{
    [SerializeField] MeshRenderer mr;
    [SerializeField] Material dontMat;
    [SerializeField] MeshRenderer bellMr;

    UnityEvent bellShotEvent = new UnityEvent();
    bool shot = false;

    public void Shot()
    {
        mr.material = dontMat;
        if (shot) return;
        GetComponent<AudioSource>().Play();
        shot = true;
        bellShotEvent.Invoke();
        FPSPlayer.FPSShake(0.1f, 10, 0.25f, 0.05f);
        BossHpBar.DisplayBossHealth();
        StartCoroutine(TextFade());
        bellMr.material.SetColor("_EmissionColor", Color.black);
    }

    IEnumerator TextFade()
    {
        Color tempColor;
        yield return new WaitForSeconds(5.0f);
        while (mr.material.color.a > 0.0f)
        {
            tempColor = mr.material.color;
            tempColor.a -= 0.05f;
            mr.material.color = tempColor;
            yield return new WaitForSeconds(0.1f);
        }
        tempColor = mr.material.color;
        tempColor.a -= 0.0f;
        mr.material.color = tempColor;
    }
}
