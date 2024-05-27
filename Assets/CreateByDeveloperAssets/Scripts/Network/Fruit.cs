
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Fruit : NetworkBehaviour
{
    public FruitData data;
    public int TileIndex;
    NetworkObjectHandler networkObjectHandler;
    [SerializeField] Animator animator;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkObjectHandler = NetworkManager.GetComponent<NetworkObjectHandler>();
        PoolObject poolObject = networkObjectHandler.PoolManager.GetObjectFromPool(data.CollectPrefab);
        poolObject.transform.position = transform.position;
        animator.SetTrigger("Spawn");

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (IsServer)
            {
                Player playerController = other.GetComponent<Player>();
                if (playerController != null)
                {
                    playerController.CollectFruitServerRpc(NetworkObjectId, playerController.OwnerClientId);
                    GetComponent<Collider>().enabled = false;
                    DespawnFruitClientRpc();
                }
            }
        }
    }
    [ClientRpc]
    public void DespawnFruitClientRpc()
    {
        StartCoroutine(DespawnCoroutine());
    }
    IEnumerator DespawnCoroutine()
    {

        PoolObject poolObject = networkObjectHandler.PoolManager.GetObjectFromPool(data.CollectPrefab);
        poolObject.transform.position = transform.position;
        animator.SetTrigger("Despawn");
        yield return new WaitForSeconds(1);
        networkObjectHandler.TileManager.tiles[TileIndex].SendSpawnFruitCallServerRpc();

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}
