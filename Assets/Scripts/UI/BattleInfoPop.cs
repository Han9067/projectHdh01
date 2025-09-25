using GB;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
public class BattleInfoPop : UIScreen
{
    public GameObject eListPrefab;
    public Transform eListParent;
    public List<EnemyList> eLists = new List<EnemyList>();
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("BattleInfoPop", this);

    }

    private void OnDisable()
    {
        Presenter.UnBind("BattleInfoPop", this);
        InitEnemyList();
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
            case "ClickBattleStart":
                WorldCore.I.SceneFadeOut(); //페이드
                break;
            case "ClickBattleEscape":
                break;
        }
    }
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "MonInfo":
                string str = data.Get<string>();
                int[] tot = str.Split('_').Select(int.Parse).ToArray();
                Dictionary<int, int> dict = new Dictionary<int, int>();
                for (int i = 0; i < tot.Length; i++)
                {
                    if (dict.ContainsKey(tot[i]))
                        dict[tot[i]]++;
                    else
                        dict.Add(tot[i], 1);
                }
                foreach (var d in dict)
                    CreateEnemyList(d.Key, d.Value);
                break;
        }
    }

    public override void Refresh()
    {

    }
    void InitEnemyList()
    {
        foreach (var eList in eLists)
        {
            Destroy(eList.gameObject);
        }
    }
    void CreateEnemyList(int id, int cnt)
    {
        var eList = Instantiate(eListPrefab, eListParent);
        eList.GetComponent<EnemyList>().SetEnemy(id, cnt);
        eLists.Add(eList.GetComponent<EnemyList>());
    }

}