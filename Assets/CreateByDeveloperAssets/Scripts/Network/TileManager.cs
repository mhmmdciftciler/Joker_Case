using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileManager : NetworkBehaviour
{
    public Tile tilePrefab; 
    [HideInInspector]public Tile[] tiles; 
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        tiles = new Tile[spawnPoints.Length];
        if (IsServer)
        {
            SpawnTiles();
        }

    }
    private void SpawnTiles()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            tiles[i] = Instantiate(tilePrefab, spawnPoints[i]);
            tiles[i].GetComponent<NetworkObject>().Spawn();
            tiles[i].Init(i);
        }
    }
    public void AddTile(Tile tile)
    {
        tiles[tile.TileIndex.Value] = tile;
    }
}
