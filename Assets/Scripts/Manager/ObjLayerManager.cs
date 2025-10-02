using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class ObjLayerManager : AutoSingleton<ObjLayerManager>
{
    public int GetObjLayer(float y)
    {
        return (int)((80 - y) * 100);
    }
}
