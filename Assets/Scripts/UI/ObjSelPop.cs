using GB;
using UnityEngine;

public class ObjSelPop : UIScreen
{
    [SerializeField] private GameObject pop;
    private string str = "";
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("ObjSelPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("ObjSelPop", this);

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
            case "ClickObjSelBlock":
                Close();
                break;
            case "ClickObjInfo":
                UIManager.ShowPopup("ObjInfoPop");
                Presenter.Send("ObjInfoPop", "ObjInfoDataB", str);
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "MovePop":
                Vector2 mousePos = data.Get<Vector2>();
                RectTransform popRect = pop.GetComponent<RectTransform>();
                Vector2 popSize = popRect.sizeDelta;
                mousePos.x = Mathf.Clamp(mousePos.x, popSize.x * 0.5f, Screen.width - popSize.x * 0.5f);
                mousePos.y = Mathf.Clamp(mousePos.y, popSize.y * 0.5f, Screen.height - popSize.y * 0.5f);
                popRect.position = mousePos;
                break;
            case "ObjInfoDataA":
                str = data.Get<string>();
                break;
        }
    }

    public override void Refresh()
    {

    }



}