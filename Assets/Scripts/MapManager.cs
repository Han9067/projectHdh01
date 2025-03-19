using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.U2D;
using UnityEngine.UI.Extensions;

public class MapManager : View
{
    public SpriteAtlas[] worldMapAtlas;
    public Vector3[] pos;
    private void OnEnable() 
    {
        //Subscription
        Presenter.Bind("Map",this);
    }
    
    private void OnDisable() 
    {
        //Unsubscribe
        Presenter.UnBind("Map",this);
    }

    //Receiver
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "worldMap":
                Debug.Log('?');
                LoadWorldMap();
            break;
        }
    }

    void LoadWorldMap()
    {
        GameObject parent = new GameObject("WorldMap");
        // float xx = -3.5f;
        // float yy = 5f;
        for(int a = 0; a < worldMapAtlas.Length; a++)
        {
            float xx = pos[a].x;
            float yy = pos[a].y;
            for(int b = 1; b <= 48; b++)
            {
                string name = "map" + (a+1) + "_" + (b);
                // string spName = b; //스프라이트 이름-> 1~48
                GameObject obj = new GameObject(name);
                SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
                renderer.sprite = worldMapAtlas[a].GetSprite(b.ToString());
                obj.transform.SetParent(parent.transform);
                obj.transform.position = new Vector3(xx*5f,yy*5f,0);
                xx++;
                if(b > 0 && b%8 == 0)
                {
                    xx = pos[a].x;
                    yy -= 1;
                }
            }
        }
    }
}
