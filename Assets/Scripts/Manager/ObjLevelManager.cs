using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using System;

public class ObjLevelManager : AutoSingleton<ObjLevelManager>
{
    public int GetLv(int VIT, int END, int STR, int AGI, int FOR, int INT, int CHA, int LUK)
    {
        int total = (VIT + END + STR + AGI + FOR + INT + CHA + LUK) - 40;
        return total < 1 ? 1 : total;
    }
    public int GetNextExp(int Lv, double s = 30.0, double p = 2.8)
    {
        double C = 1000.0 / Math.Pow(1.0 + s, p);
        double raw = C * Math.Pow(Lv + s, p);
        return (int)(Math.Floor(raw / 10.0 + 0.5) * 10.0);
    }
    public int GetGainExp(int _hp, int _sp, int _mp, int _str, int _agi, int _int, int _cha, int _luk)
    {
        double score = _hp + (_sp + _mp) * 1 + (_str + _agi) * 0.8 + (_int + _cha + _luk) * 0.5;
        return (int)(score * 4);
    }
}
