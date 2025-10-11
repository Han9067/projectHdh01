using GB;
using UnityEngine;


public class JournalPop : UIScreen
{
    [SerializeField] private Transform parent;


    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("JournalPop", this);
    }

    private void OnDisable()
    {
        Presenter.UnBind("JournalPop", this);

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
            case "OnJnClose":
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