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
    [SerializeField] private GameObject cursor;
    public Sprite unwalkableSprite;
    private Camera mainCamera;
    private bool ClickOn = false;
    void Start()
    {
        // UIManager.I.Init();

        GameStart();

        //Vector3 worldPosition = tilemap.CellToWorld(tilePosition) + tilemap.tileAnchor;
            // player.position = worldPosition;
        // Presenter.Send("Player","tile",tile);
        // Presenter.Send("Player","pos",)
    }
    private void GameStart(){
        mainCamera = Camera.main;
        Vector3Int tilePosition = new Vector3Int(0, 0, 0);
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
            // Debug.Log(ClickOn);

            Vector3 startPos = new Vector3Int(0, 0, 0);  // 시작 위치 (월드 좌표)
            Vector3 targetPos = new Vector3Int(2, 2, 0);  // 목표 위치 (월드 좌표)
            List<Vector3> path = PathManager.I.FindPath(startPos, targetPos, tilemap);
            Debug.Log(path);
            if (path != null)
            {
                foreach (Vector3 step in path)
                {
                    Debug.Log($"Path Step: {step}");
                }
            }
            else
            {
                Debug.Log("경로를 찾을 수 없습니다.");
            }
        }
        
        if (IsMouseOverTile(out Vector3Int tilePosition)){
            if(cursor.transform.position.x == tilePosition.x + 0.5f && cursor.transform.position.y == tilePosition.y + 0.5f)return;

            cursor.transform.position = new Vector3(tilePosition.x + 0.5f,tilePosition.y + 0.5f, 0);

            // Debug.Log(tilePosition);

            TileBase tile = tilemap.GetTile(tilePosition);
            if (tile != null && tile is Tile currentTile)
            {
                if (currentTile.sprite == unwalkableSprite){
                    Debug.Log("이동 불가 타일");
                    ClickOn = false;
                }else{
                    ClickOn = true;
                    // Vector3 startPos = new Vector3(0, 0, 0);  // 시작 위치 (월드 좌표)
                    // Vector3 targetPos = new Vector3(5, 5, 0);  // 목표 위치 (월드 좌표)
                    // List<Vector3> path = AStarPathfinding.Instance.FindPath(tilemap, startPos, targetPos);
                    // Debug.Log(path);
                }
                cursor.SetActive(ClickOn);
            }
        }

        // // Debug.Log(Input.mousePosition);
        // if(Input.GetKeyDown(KeyCode.Q))
        // {
        //     Debug.Log("??");
        // }
    }
}
