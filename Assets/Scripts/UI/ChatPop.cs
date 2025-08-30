using GB;


public class ChatPop : UIScreen
{
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("ChatPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("ChatPop", this);

    }

    public void RegistButton()
    {
        foreach (var v in mButtons)
            v.Value.onClick.AddListener(() => { OnButtonClick(v.Key); });

    }

    public void OnButtonClick(string key)
    {
        // switch (key)
        // {
        // }
    }
    public override void ViewQuick(string key, IOData data)
    {

    }

    public override void Refresh()
    {

    }

}