using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GameNetworkManager : NetworkBehaviour
{
    public NetworkVariable<int> PlayOrder = new NetworkVariable<int>();
    public NetworkVariable<ulong> Player = new NetworkVariable<ulong>();//For display. Can be delete.
    [SerializeField] private Dice dice1;//The list must be added to increase the number of dice.
    [SerializeField] private Dice dice2;
    public NetworkVariable<PlayerStatus> _playerStatus = new NetworkVariable<PlayerStatus>();
    public UnityEvent OnPlayOrder;
    public UnityEvent OnPlayNotOrder;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            StartCoroutine(SetFirstPlayerOrder());
            StartCoroutine(CheckPlayerConnection());
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void ThrowDiceServerRpc(ulong playerId, int dice1, int dice2)//The list must be added to increase the number of dice.
    {
        if (IsServer)
        {
            if (dice1 > 6 || dice1 <= 0 || dice2 > 6 || dice2 <= 0)//Game Rule
                return;

            if (Player.Value != playerId || _playerStatus.Value != PlayerStatus.PlayerIsNotPlaying)
            {
                return;
            }
            _playerStatus.Value = PlayerStatus.PlayerIsPlaying;
            //This current player can no longer send a request to roll dice. To report this, the player is notified that he is not eligible to roll dice.
            SendPlayerOrderStatusClientRpc(false, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { Player.Value }
                }
            });
            StartCoroutine(ThrowDiceCoroutine(dice1, dice2));
        }
        else
        {
            Debug.LogError("Is not Server");
        }
    }
    
    IEnumerator ThrowDiceCoroutine(int dice1, int dice2)
    {
        this.dice1.ThrowDiceServerRpc(dice1);
        this.dice2.ThrowDiceServerRpc(dice2);
        yield return new WaitForSeconds(3);
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(Player.Value, out NetworkClient client))
        {
            client.PlayerObject.GetComponent<Player>().CalculateTargetAndMoveServerRpc(dice1 + dice2);
        }
        _playerStatus.Value = PlayerStatus.PlayerIsPlayed;

        if ((int)(PlayOrder.Value + 1) < NetworkManager.Singleton.ConnectedClients.Count)
        {
            PlayOrder.Value++;
        }
        else
        {
            PlayOrder.Value = 0;
        }
        Player.Value = NetworkManager.Singleton.ConnectedClientsIds[PlayOrder.Value];
        _playerStatus.Value = PlayerStatus.PlayerIsNotPlaying;
        //The new player can now roll dice. It tells the player that it is possible to roll the dice to declare this.
        SendPlayerOrderStatusClientRpc(true, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { Player.Value }
            }
        });
    }

    [ClientRpc(AllowTargetOverride = true)]
    private void SendPlayerOrderStatusClientRpc(bool status, ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(SendPlayerOrderStatus(status));

    }
    IEnumerator SendPlayerOrderStatus(bool status)
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Status : " + status);
        if (status)
        {
            OnPlayOrder?.Invoke();
        }
        else
        {
            OnPlayNotOrder?.Invoke();
        }
    }
    public List<ulong> GetAllClientIdsExcept(ulong excludedClientId)
    {
        List<ulong> clientIds = new List<ulong>();

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != excludedClientId)
            {
                clientIds.Add(clientId);
            }
        }

        return clientIds;
    }
    private IEnumerator CheckPlayerConnection()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(Player.Value))
            {
                Debug.Log($"Player {Player.Value} disconnected.");
                if ((int)(PlayOrder.Value + 1) < NetworkManager.Singleton.ConnectedClients.Count)
                {
                    PlayOrder.Value++;
                }
                else
                {
                    PlayOrder.Value = 0;
                }
                //The new player can now roll dice. It tells the player that it is possible to roll the dice to declare this.
                SendPlayerOrderStatusClientRpc(true, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { Player.Value }
                    }
                });
                yield break;
            }
        }
    }
    private IEnumerator SetFirstPlayerOrder()
    {
        yield return new WaitForSeconds(2);
        Player.Value = NetworkManager.Singleton.ConnectedClientsIds[0];
        PlayOrder.Value = 0;
        SendPlayerOrderStatusClientRpc(true, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {Player.Value}
            }
        });
    }
}
public enum PlayerStatus
{
    PlayerIsNotPlaying,
    PlayerIsPlaying,
    PlayerIsPlayed,
    PlayerDisconnect
}