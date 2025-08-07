using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class bPlayer : View
{
    public int gx, gy;
    public override void ViewQuick(string key, IOData data)
    {
        switch (key)
        {
            case "SetGridPos":
                string[] strArr = data.Get<string>().Split('_');
                gx = int.Parse(strArr[0]); gy = int.Parse(strArr[1]);
                break;
        }
    }
}
