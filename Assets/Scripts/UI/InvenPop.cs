using GB;
using UnityEngine;


public class InvenPop : UIScreen
{

    

    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("InvenPop",this);
    }

    private void OnDisable() 
    {
        Presenter.UnBind("InvenPop", this);

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
            case "Close":
                if (ShopInvenPop.isActive)
                {
                    UnityEngine.Debug.Log("ShopInvenPop이 활성화되어 있어서 InvenPop을 닫을 수 없습니다.");
                    return;
                }
                Close();
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