using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectHandler : MonoBehaviour
{
    public TileManager TileManager;
    public FruitSpawner FruitSpawner;
    public int poolSize;
    public PoolManager PoolManager;
    public GameNetworkManager gameNetworkManager;
    public UIInventory Inventory;
    public UIPlayerInfo UIPlayerInfo;
}
