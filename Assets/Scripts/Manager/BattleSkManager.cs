using UnityEngine;
using GB;

public class BattleSkManager : AutoSingleton<BattleSkManager>
{
    public int consumeVal = 0;
    public void InitBtSk()
    {
        BattleCore.I.InitBtSk();
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
        BattleCore.I.BeginSkill(skId, 0);
        if (!PlayerManager.I.pData.SkList.TryGetValue(skId, out SkData data))
            return;
        Vector2Int from = BattleCore.I.GetPlayerTilePos();
        switch (skId)
        {
            case 1002:
                BattleCore.I.BeginSkill(skId, 1);
                BattleCore.I.ShowAttRng(from, 1, 1, GetSkAttVal(data, 608));
                consumeVal = GetSkAttVal(data, 57);
                break;
            case 1003:
                BattleCore.I.BeginSkill(skId, 2);
                BattleCore.I.ShowAttRng(from, 1, 1, PlayerManager.I.pData.Rng);
                consumeVal = GetSkAttVal(data, 57);
                break;
            case 1004:
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
                break;
            case 1002:
                PlayerManager.I.pData.SP -= consumeVal;
                Presenter.Send("BattleMainUI", "GetPlayerSp");
                BattleCore.I.DashToTile(pos);
                break;
            case 1003:
                PlayerManager.I.pData.SP -= consumeVal;
                Presenter.Send("BattleMainUI", "GetPlayerSp");
                BattleCore.I.ActMeleeToTile(pos, 1003);
                break;
        }
        InitBtSk();
    }

    public static int GetSkAttVal(SkData data, int attId)
    {
        if (data?.Att == null) return 0;
        foreach (var at in data.Att)
            if (at.AttID == attId) return at.Val;
        return 0;
    }
}
