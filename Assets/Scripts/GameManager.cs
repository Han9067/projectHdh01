using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class GameManager : AutoSingleton<GameManager>
{
    private void Awake() {
        if(I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public enum GameMode{InGame,Intro}
    public GameMode mode;

}
