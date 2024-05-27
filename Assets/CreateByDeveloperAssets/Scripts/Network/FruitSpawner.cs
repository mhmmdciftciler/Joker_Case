using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FruitSpawner : NetworkBehaviour
{

    public NetworkObject[] fruitPrefabs;
    public float fruitRespawnTime;
    [SerializeField] TileManager tileManager;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    public void SpawnFruit(int tileIndex)
    {
        Debug.Log("Spawn Tile : " + tileIndex);
        NetworkObject fruit = Instantiate(fruitPrefabs[Random.Range(0,fruitPrefabs.Length)], tileManager.spawnPoints[tileIndex]);
        fruit.transform.localPosition = Vector3.up;
        fruit.GetComponent<Fruit>().TileIndex = tileIndex;
        fruit.Spawn();
    }

}
