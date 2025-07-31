using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class BattleMainUI : UIScreen
{
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("BattleMainUI",this);
    }

    private void OnDisable() 
    {
        Presenter.UnBind("BattleMainUI", this);

    }

    public void RegistButton()
    {
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
        
    }

    public void OnButtonClick(string key)
    {
        if (key.Contains("State"))
        {
            GameObject go = null;
            string str = "";
            
            switch (key)
            {
                case "StateCharInfoPop":
                    go = GameObject.Find("CharInfoPop");
                    str = "CharInfoPop";
                    break;
                case "StateInvenPop":
                    go = GameObject.Find("InvenPop");
                    str = "InvenPop";
                    break;
                case "StateQuestPop":
                    go = GameObject.Find("QuestPop");
                    str = "QuestPop";
                    break;
                case "StateSkillPop":
                    go = GameObject.Find("SkillPop");
                    str = "SkillPop";
                    break;
            }
            
            if (go == null)
            {
                UIManager.ShowPopup(str);
            }
            else
            {
                if (go.gameObject.activeSelf)
                    UIManager.ClosePopup(str);
                else
                    UIManager.ShowPopup(str);
            }
        }
        else{
            switch(key){
                case "GoToWorld":
                    UnityEngine.Debug.Log("GoToWorld");
                    break;
            }
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
         
    }

    public override void Refresh()
    {  
    }



}