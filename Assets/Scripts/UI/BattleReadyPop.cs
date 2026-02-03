using GB;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleReadyPop : UIScreen
{
    public Transform eListParent;
    public List<EnemyList> eLists = new List<EnemyList>();
    private void Awake()
    {
        Regist();
        RegistButton();
    }

    private void OnEnable()
    {
        Presenter.Bind("BattleReadyPop", this);

    }

    private void OnDisable()
    {
        Presenter.UnBind("BattleReadyPop", this);
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
        GameObject eList = Instantiate(ResManager.GetGameObject("EnemyList"), eListParent);
        EnemyList enemyList = eList.GetComponent<EnemyList>();
        enemyList.SetEnemy(id, cnt);
        eLists.Add(enemyList);
    }
    public override void BackKey()
    {
        return;
        // base.BackKey();
    }
}