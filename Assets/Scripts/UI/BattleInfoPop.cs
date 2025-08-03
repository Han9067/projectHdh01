using GB;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleInfoPop : UIScreen
{
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("BattleInfoPop",this);
    }

    private void OnDisable() 
    {
        Presenter.UnBind("BattleInfoPop", this);

    }

    public void RegistButton()
    {
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
    }

    public void OnButtonClick(string key)
    {
        switch(key)
        {
            case "ClickBattleStart":
                WorldCore.I.SceneFadeOut(); //페이드
                break;
            case "ClickBattleEscape":
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "MonInfo":
                // string str = data.Get<string>();
                // int[] strArr = str.Split('_').Select(int.Parse).ToArray();
                //추후 몬스터 정보 기입 필요
                break;
        }
    }

    public override void Refresh()
    {
            
    }



}