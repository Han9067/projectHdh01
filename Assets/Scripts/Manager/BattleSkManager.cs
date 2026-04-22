using UnityEngine;
using GB;

public class BattleSkManager : AutoSingleton<BattleSkManager>
{
    private SkData curSkData = null;
    private int csmHp = 0, csmMp = 0, csmSp = 0; //소모 체력, 마나, 스페셜
    public void InitBtSk()
    {
        BattleCore.I.InitBtSk();
        curSkData = null;
    }
    public void StateSk(int skId)
    {
        GsManager.I.InitCursor();
        BattleCore.I.GetSkillState(out bool isSk, out int curSkId, out _, out _);
        if (isSk)
            InitBtSk();
        else
        {
            if (curSkId == skId) return;
            ClickSk(skId);
        }
    }
    public void ClickSk(int skId)
    {
        if (!PlayerManager.I.pData.SkList.TryGetValue(skId, out SkData data))
            return;
        curSkData = data;
        switch (curSkData.UseType)
        {
            case 1:
                csmHp = GetSkAttVal(curSkData, 55);
                if (PlayerManager.I.pData.HP < csmHp)
                {
                    GsManager.I.ShowTstMsg("Tst_NotSk");
                    return;
                }
                break; //hp 소모
            case 2:
                csmMp = GetSkAttVal(curSkData, 56);
                if (PlayerManager.I.pData.MP < csmMp)
                {
                    GsManager.I.ShowTstMsg("Tst_NotSk");
                    return;
                }
                break; //mp 소모
            case 3:
                csmSp = GetSkAttVal(curSkData, 57);
                if (PlayerManager.I.pData.SP < csmSp)
                {
                    GsManager.I.ShowTstMsg("Tst_NotSk");
                    return;
                }
                break; //sp 소모
        }
        Vector2Int from = BattleCore.I.GetPlayerTilePos();
        switch (skId)
        {
            case 1001:
                BattleCore.I.BeginSkill(skId, 0);
                BattleCore.I.UseSk(skId);
                break;
            case 1002:
                BattleCore.I.BeginSkill(skId, 1);
                // BattleCore.I.ShowAttRng(from, 1, 1, 3);
                break;
            case 1003:
                BattleCore.I.BeginSkill(skId, 2);
                // BattleCore.I.ShowAttRng(from, 1, 1, 3);
                break;
            case 1101:
            case 1201:
            case 1202:
            case 1301:
                BattleCore.I.BeginSkill(skId, 2);
                // BattleCore.I.ShowAttRng(from, 1, 1, 1);
                break;
            case 1401:
                BattleCore.I.BeginSkill(skId, 2);
                // BattleCore.I.ShowAttRng(from, 1, 1, 2);
                break;
        }
    }

    public bool IsUsingSk()
    {
        BattleCore.I.GetSkillState(out _, out _, out Vector2Int skPos, out bool available);
        if (!available)
        {
            GsManager.I.ShowTstMsg("Tst_NotSk");
            return false;
        }
        if (!BattleCore.I.GetActiveCurPosWithRngGrid(skPos))
        {
            InitBtSk();
            return false;
        }
        return true;
    }

    public void ActSkill(int skId, Vector2Int pos)
    {
        switch (skId)
        {
            case 1001:
                BattleCore.I.BuffSk(skId);
                float mpVal = GetSkAttVal(curSkData, 402) * 0.01f;
                float spVal = GetSkAttVal(curSkData, 403) * 0.01f;
                PlayerManager.I.pData.MP += (int)(PlayerManager.I.pData.MP * mpVal);
                PlayerManager.I.pData.SP += (int)(PlayerManager.I.pData.SP * spVal);
                if (PlayerManager.I.pData.MP > PlayerManager.I.pData.MaxMP)
                    PlayerManager.I.pData.MP = PlayerManager.I.pData.MaxMP;
                if (PlayerManager.I.pData.SP > PlayerManager.I.pData.MaxSP)
                    PlayerManager.I.pData.SP = PlayerManager.I.pData.MaxSP;
                Presenter.Send("BattleMainUI", "UpdateInfo");
                break;
            case 1002:
                PlayerManager.I.pData.SP -= csmSp;
                Presenter.Send("BattleMainUI", "GetPlayerSp");
                BattleCore.I.DashToTile(pos);
                break;
            case 1003:
                break;
            case 1101:
            case 1201:
            case 1301:
                PlayerManager.I.pData.SP -= csmSp;
                Presenter.Send("BattleMainUI", "GetPlayerSp");
                BattleCore.I.ActMeleeToTile(pos, skId);
                break;
        }
        //스킬 쿨타임 시작
        curSkData.CurCt = curSkData.SkCt;
        // Debug.Log(curSkData.CurCt);

        InitBtSk(); //초기화
    }

    public static int GetSkAttVal(SkData data, int attId)
    {
        if (data?.Att == null) return 0;
        foreach (var at in data.Att)
        {
            if (at.AttID == attId) return at.Val;
        }
        return 0;
    }
}
