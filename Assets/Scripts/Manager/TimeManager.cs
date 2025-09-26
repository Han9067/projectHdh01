using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class TimeManager : AutoSingleton<TimeManager>
{
    public int wYear, wMonth, wDay;
    public float wTime;
    void Start()
    {
        wYear = 316;
        wMonth = 4;
        wDay = 1;
        wTime = 0; //wTime의 수치가 일정 이상 올라가면 일차가 올라감.
                   //저장된 데이터가 있다면 추후 불러와 년,월,일을 적용
        Presenter.Send("WorldMainUI", "UpdateTime");
    }
    void Update()
    {
        wTime += Time.deltaTime;
        if (wTime >= 120.0f)
        {
            wTime = 0;
            AddDay();
        }
    }
    public void AddDay()
    {
        wDay++;
        if (wDay > 30)
        {
            wDay = 1;
            wMonth++;
            if (wMonth > 12)
            {
                wMonth = 1;
                wYear++;
            }
        }
        Presenter.Send("WorldMainUI", "UpdateTime");
    }
}
