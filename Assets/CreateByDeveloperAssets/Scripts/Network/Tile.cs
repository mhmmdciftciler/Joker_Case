using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    public NetworkVariable<int> PlayerCount = new NetworkVariable<int>();
    public NetworkVariable<int> TileIndex = new NetworkVariable<int>();
    [SerializeField] TextMeshPro timeText;

    FruitSpawner FruitSpawner;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            FruitSpawner = NetworkManager.gameObject.GetComponent<NetworkObjectHandler>().FruitSpawner;
        }
        else
        {
            NetworkManager.gameObject.GetComponent<NetworkObjectHandler>().TileManager.AddTile(this);
        }
        StartCoroutine(SpawnedTile());
    }

    [ServerRpc(RequireOwnership =false)]
    public void SendSpawnFruitCallServerRpc()
    {
        if (TileIndex.Value > 0)
            StartCoroutine(SendSpawnFruit());

    }
    IEnumerator SendSpawnFruit()
    {
        yield return new WaitForSeconds(10);
        if(PlayerCount.Value == 0)
        FruitSpawner.SpawnFruit(TileIndex.Value);
    }
    IEnumerator SpawnedTile()
    {
        yield return new WaitForSeconds(0.3f);
        if(TileIndex.Value > 0)
        {
            timeText.text = TileIndex.Value.ToString();
            if(IsServer)
            FruitSpawner.SpawnFruit(TileIndex.Value);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerOnServerRpc(bool status)
    {
        PlayerCount.Value = status ? PlayerCount.Value + 1 : PlayerCount.Value - 1;
        if (PlayerCount.Value < 0)
        {
            Debug.LogError("Opps!");
            PlayerCount.Value = 0;  
        }
    }
    internal void Init(int i)
    {
        if (IsServer)
        {
            TileIndex.Value = i;
        }
    }
}

public enum TileStatus
{
    HasNotFruit,
    WaitingRespawnTime,
    HasFruit
}