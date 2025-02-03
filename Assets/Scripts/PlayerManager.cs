using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using NaughtyAttributes;

public class PlayerManager : MonoBehaviour
{
    
    [Button]
    void Push()
    {
        // _playerData = new PlayerData();
        // _playerData.HP = 100;

        // Presenter.Send("Player","Data");
        Presenter.Send("Player","Test");
        // Presenter.Send("Player","Data",_playerData);
    }

}
