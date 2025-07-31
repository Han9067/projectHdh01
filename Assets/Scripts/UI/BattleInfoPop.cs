using GB;
using UnityEngine;
using System.Collections.Generic;

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
        // switch(key)
        // {
        // }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "MonInfo":
                Debug.Log("MonInfo");
                // List<int> monParty = data.Get<List<int>>("MonParty");
                // List<int> monParty = data.Get<List<int>>("MonParty");
                // Debug.Log(monParty.Count);
                break;
        }
    }

    public override void Refresh()
    {
            
    }



}