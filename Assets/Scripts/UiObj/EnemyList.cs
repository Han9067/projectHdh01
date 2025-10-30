using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;
public class EnemyList : MonoBehaviour
{
    public Image eIcon;
    public Text eName;

    public void SetEnemy(int id, int cnt)
    {
        //mIcon_1
        eIcon.sprite = ResManager.GetSprite($"mIcon_{id}");
        eName.text = MonManager.I.MonDataList[id].Name + " (" + cnt + ")";
    }
}
