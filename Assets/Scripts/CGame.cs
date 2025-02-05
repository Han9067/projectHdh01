using UnityEngine;
using GB;
using UnityEngine.Tilemaps;
using DG.Tweening;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class CGame : AutoSingleton<CGame>
{

    // [SerializeField] private Tilemap tilemap;
    public Tilemap tilemap;
    [SerializeField] private GameObject player; //플레이어
    [SerializeField] private GameObject cursor; //커서 오브젝트
    public Sprite unwalkableSprite; //이동 불가 스프라이트
    private Camera mainCamera; //메인 카메라
    private bool ClickOn = false; //마우스로 이동한 좌표에 플레이어가 이동가능한 좌표일때, 해당 불대수가 활성화됨
    private List<Vector3> path; //a* 알고리즘으로 검색한 좌표들 저장하는 리스트
    void Start()
    {
        // UIManager.I.Init();

        GameStart();
    }
    private void GameStart(){
        mainCamera = Camera.main;
        // Vector3Int tilePosition = new Vector3Int(0, 0, 0);
        // GB.Presenter.Send("Test2Script","A",1);
        // GB.Presenter.Send("Player","Init");
    }
    private bool IsMouseOverTile(out Vector3Int tilePosition)
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        tilePosition = tilemap.WorldToCell(mouseWorldPos);
        return tilemap.HasTile(tilePosition);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 startPos = new Vector3Int(cursor.x,cursor.y)

            //Vector3Int pPos = tilemap.WorldToCell(player.transform.position);
            //tilePosition
            // List<Vector3> path = PathManager.I.FindPath(player.transform.position, cursor.transform.position, tilemap);
            // if (path != null)
            // {
            //     foreach (Vector3 step in path)
            //     {
            //         Debug.Log($"Path Step: {step}");
            //     }
            // }
            if (path != null)
            {
                // foreach (Vector3 step in path)
                // {
                //     Debug.Log($"Path Step: {step}");
                // }
                GB.Presenter.Send("Player","Move",path);
            }
        }
        
        if (IsMouseOverTile(out Vector3Int tilePosition)){
            if(cursor.transform.position.x == tilePosition.x + 0.5f && cursor.transform.position.y == tilePosition.y + 0.5f)return;

            cursor.transform.position = new Vector3(tilePosition.x + 0.5f,tilePosition.y + 0.5f, 0);

            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile != null && tile is Tile currentTile)
            {
                if (currentTile.sprite == unwalkableSprite){
                    // Debug.Log("이동 불가 타일");
                    ClickOn = false;
                    
                }else{
                    ClickOn = true;
    
                    path = PathManager.I.FindPath(player.transform.position, cursor.transform.position, tilemap);
                    // Debug.Log(path.Count);
                }
                cursor.SetActive(ClickOn);
            }
        }
    }
}
