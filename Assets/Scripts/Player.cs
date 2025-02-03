using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class Player : View
{
    public int px,py;    

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
            Debug.Log(px);
            Debug.Log(py);

            //value = 1
            // int value = ODataConverter.Convert<int>(data);
            // Debug.Log(value);
            break;

        }
        
    }
}
