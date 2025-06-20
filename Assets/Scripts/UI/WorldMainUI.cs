using System.Diagnostics;
using GB;
using Unity.VisualScripting;
using UnityEngine;


public class WorldMainUI : UIScreen
{



    private void Awake()
    {
        Regist();
        RegistButton();

    }

    private void OnEnable()
    {
        Presenter.Bind("WorldMainUI",this);
    }

    private void OnDisable() 
    {
        Presenter.UnBind("WorldMainUI", this);

    }

    public void RegistButton()
    {
        foreach(var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key);});
        
    }

    public void OnButtonClick(string key)
    {
        GameObject go = null;
        string str = "";
        switch (key)
        {
            case "stateChar":
                go = GameObject.Find("CharInfoPop");
                str = "CharInfoPop";
                break;
            case "stateInven":
                go = GameObject.Find("InvenPop");
                str = "InvenPop";
                break;
            case "stateQuest":
                go = GameObject.Find("QuestPop");
                str = "QuestPop";
                break;
            case "stateSkill":
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
    public override void ViewQuick(string key, IOData data)
    {
         
    }

    public override void Refresh()
    {
            
    }



}