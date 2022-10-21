using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHpBar : MonoBehaviour
{
    public static BossHpBar inst;

    [SerializeField] Transform barFront;
    [SerializeField] Transform barBack;

    float backBarTimer = 0;
    float backBarFill = 1.0f;
    float backBarIFill = 1.0f;
    float fill = 1.0f;

    public void UpdateHealth()
    {
        fill = VRPlayerController.main.health / 100.0f;
        if (fill < 0) fill = 0;
        barFront.localScale = new Vector3(fill, 1, 1);
        backBarTimer = 0.8f;
    }

    private void Awake()
    {
        inst = this;
        gameObject.SetActive(false);
    }

    public static void DisplayBossHealth()
    {
        if (inst != null) inst.gameObject.SetActive(true);
    }

    private void Update()
    {
        barBack.localScale = new Vector3(backBarFill, 1, 1);
        backBarFill = Mathf.MoveTowards(backBarFill, backBarIFill, Time.deltaTime*0.5f);
        if (backBarTimer <= 0) return;
        backBarTimer -= Time.deltaTime;
        if (backBarTimer <= 0)
        {
            backBarIFill = fill;
        }
    }

    private void OnEnable()
    {
        VRPlayerController.isHurtEvent += UpdateHealth;
    }

    private void OnDisable()
    {
        VRPlayerController.isHurtEvent -= UpdateHealth;
    }

}
