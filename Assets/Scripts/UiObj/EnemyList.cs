using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GB;
using TMPro;

public class EnemyList : MonoBehaviour
{
    public Image eIcon;
    public TextMeshProUGUI eName;

    public void SetEnemy(int id, int cnt)
    {
        //mIcon_1
        eIcon.sprite = ResManager.GetSprite($"mIcon_{id}");
        eName.text = LocalizationManager.GetValue(MonManager.I.MonDataList[id].Name) + " (" + cnt + ")";
    }
}
