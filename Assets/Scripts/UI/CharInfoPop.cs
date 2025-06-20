using GB;


public class CharInfoPop : UIScreen
{

    

    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("CharInfoPop",this);
    }

    private void OnDisable() 
    {
        Presenter.UnBind("CharInfoPop", this);

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
            case "closePop":
                UIManager.ClosePopup("CharInfoPop");
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