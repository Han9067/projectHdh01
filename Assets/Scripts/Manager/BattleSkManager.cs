using UnityEngine;
using GB;

public class BattleSkManager : AutoSingleton<BattleSkManager>
{
    private SkData curSkData = null;
    private int curSkIdx = -1;
    private int csmHp = 0, csmMp = 0, csmSp = 0; //소모 체력, 마나, 스페셜
    public void InitBtSk()
    {
        BattleCore.I.InitBtSk();
        curSkData = null;
        curSkIdx = -1;
        csmHp = 0; csmMp = 0; csmSp = 0;
    }
    public void ClickSk(int skId, int btnIdx)
    {
        GsManager.I.InitCursor();
        if (!PlayerManager.I.pData.SkList.TryGetValue(skId, out SkData data))
            return;
        if (curSkIdx == btnIdx || BattleCore.I.isSk)
        {
            InitBtSk();
            return;
        }
        if (data.CurCt > 0)
        {
            GsManager.I.ShowTstMsg("Tst_NotSkCt");
            return;
        }
        curSkIdx = btnIdx;
        curSkData = data;
        switch (curSkData.UseType)
        {
            case 1:
                csmHp = GetSkAttVal(curSkData, 55);
                if (PlayerManager.I.pData.HP < csmHp)
                {
                    GsManager.I.ShowTstMsg("Tst_NotEnoughN", "HP");
                    return;
                }
                break; //hp 소모
            case 2:
                csmMp = GetSkAttVal(curSkData, 56);
                if (PlayerManager.I.pData.MP < csmMp)
                {
                    GsManager.I.ShowTstMsg("Tst_NotEnoughN", "MP");
                    return;
                }
                break; //mp 소모
            case 3:
                csmSp = GetSkAttVal(curSkData, 57);
                if (PlayerManager.I.pData.SP < csmSp)
                {
                    GsManager.I.ShowTstMsg("Tst_NotEnoughN", "SP");
                    return;
                }
                break; //sp 소모
        }
        Vector2Int from = BattleCore.I.GetPlayerTilePos();
        switch (skId)
        {
            case 1001:
                BattleCore.I.BeginSkill(skId);
                BattleCore.I.UseSk(skId);
                break;
            case 1002:
                BattleCore.I.BeginSkill(skId);
                BattleCore.I.ShowSkRng(3, from, 1, 2);
                //rng : 범위, pos : 스킬 사용자 위치, sk : 스킬 타입(단일, 다중 등), tg : 타겟 타입(적, 자신, 아군 버프 등)
                break;
            case 1003:
                BattleCore.I.BeginSkill(skId);
                break;
            case 1101:
            case 1201:
            case 1202:
            case 1301:
                //주변 1칸 사각형으로 단일 공격
                BattleCore.I.BeginSkill(skId);
                BattleCore.I.ShowSkRng(1, from, 1, 1);
                break;
            case 1401:
                BattleCore.I.BeginSkill(skId);
                // BattleCore.I.ShowAttRng(from, 1, 1, 2);
                break;
        }
    }

    public bool IsUsingSk()
    {
        if (!BattleCore.I.GetActiveCurPosWithRngGrid(BattleCore.I.GetSelSkPos()))
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
        if (PlayerManager.isZeroCt)
            curSkData.CurCt = 0;
        int[] arr = { curSkIdx, curSkData.CurCt };
        Presenter.Send("BattleMainUI", "UpdateSkCt", arr);
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
