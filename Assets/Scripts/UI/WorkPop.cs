using GB;


public class WorkPop : UIScreen
{



    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("WorkPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("WorkPop", this);

    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });

    }

    public void OnButtonClick(string key)
    {
        switch (key)
        {
            case "ClickBlock":
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