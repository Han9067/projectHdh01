using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class Player : View
{
    public int px,py;    
    private List<Vector3> movePath;
    private void OnEnable() 
    {
        //Subscription
        Presenter.Bind("Player",this);
        
    }
    
    private void OnDisable() 
    {
        //Unsubscribe
        Presenter.UnBind("Player",this);
    }

    //Receiver
    public override void ViewQuick(string key, IOData data)
    {
        switch(key)
        {
            case "Init":
                // Debug.Log(px);
                // Debug.Log(py);

                //value = 1
                // int value = ODataConverter.Convert<int>(data);
                // Debug.Log(value);
            break;
            case "Move":
                movePath = ODataConverter.Convert<List<Vector3>>(data);
                foreach (Vector3 step in movePath)
                {
                    Debug.Log($"Path Step: {step}");
                }

                // Debug.Log("??");
            break;
        }
        
    }
}
