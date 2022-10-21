using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCanvas : MonoBehaviour
{
    public void StartGame()
    {
        FPSPlayer.StartGame();
        Destroy(gameObject);
    }
}
