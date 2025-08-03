using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BattleCore : AutoSingleton<BattleCore>
{
    public Image blackImg;
    void Awake()
    {
        if(!blackImg.gameObject.activeSelf)
            blackImg.gameObject.SetActive(true);
        blackImg.color = new Color(0, 0, 0, 1f);
        blackImg.DOFade(0f, 1f).OnComplete(() => {
            Debug.Log("BattleCore Start");
            blackImg.gameObject.SetActive(false);
        });
        //그리드 생성
        CreateGrid();
    }
    void Start()
    {
    }
    void CreateGrid()
    {
        //그리드 생성

    }
}
