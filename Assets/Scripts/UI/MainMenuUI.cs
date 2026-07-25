using GB;


public class MainMenuUI : UIScreen
{



    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("MainMenuUI", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("MainMenuUI", this);

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
            case "NewGame":
                break;
            case "LoadGame":
                break;
            case "Settings":
                break;
            // case "Credits":
            //     break;
            case "LoadMap":
                break;
            case "Exit":
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetNewChar":
                break;
            case "OpenSaveSlot":
                break;
            case "OpenSettings":
                break;
        }
    }

    public override void Refresh()
    {

    }



}