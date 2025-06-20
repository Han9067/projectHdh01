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
        switch (key)
        {
            case "stateCharPop":
                GameObject go = GameObject.Find("CharInfoPop");
                if (go == null)
                {
                    UIManager.ShowPopup("CharInfoPop");
                }
                else
                {
                    if (go.gameObject.activeSelf)
                        UIManager.ClosePopup("CharInfoPop");
                    else
                        UIManager.ShowPopup("CharInfoPop");
                }
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
         
    }

    public override void Refresh()
    {
            
    }



}