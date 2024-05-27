using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GameNetworkManager : NetworkBehaviour
{
    public NetworkVariable<int> playOrder = new NetworkVariable<int>();
    public NetworkVariable<ulong> player = new NetworkVariable<ulong>();//For display. Can be delete.
    [SerializeField] private Dice dice1;//The list must be added to increase the number of dice.
    [SerializeField] private Dice dice2;
    private NetworkVariable<PlayerStatus> _playerStatus = new NetworkVariable<PlayerStatus>();
    public UnityEvent OnPlayOrder;
    public UnityEvent OnPlayNotOrder;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            StartCoroutine(SetFirstPlayerOrder());
        }
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectServerRpc;

    }
    [ServerRpc(RequireOwnership =false)]
    private void OnClientDisconnectServerRpc(ulong obj)
    {
        if(player.Value == obj)
        {
            Debug.LogError($"Player {player.Value} disconnected.");

            //The new player can now roll dice. It tells the player that it is possible to roll the dice to declare this.

            if ((int)(playOrder.Value + 1) < NetworkManager.Singleton.ConnectedClients.Count) 
            {
                playOrder.Value++;
            }
            else
            {
                playOrder.Value = 0;
            }
            player.Value = NetworkManager.Singleton.ConnectedClientsIds[playOrder.Value];
            SendPlayerOrderStatusClientRpc(true, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { player.Value }
                }
            });
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void ThrowDiceServerRpc(ulong playerId, int dice1, int dice2, int throwCount)//The list must be added to increase the number of dice.
    {
        if (IsServer)
        {
            if (dice1 > 6 || dice1 <= 0 || dice2 > 6 || dice2 <= 0 || throwCount<1)//Game Rule
                return;

            if (player.Value != playerId || _playerStatus.Value != PlayerStatus.PlayerIsNotPlaying)
            {
                return;
            }
            _playerStatus.Value = PlayerStatus.PlayerIsPlaying;
            //This current player can no longer send a request to roll dice. To report this, the player is notified that he is not eligible to roll dice.
            SendPlayerOrderStatusClientRpc(false, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { player.Value }
                }
            });
            StartCoroutine(ThrowDiceCoroutine(dice1, dice2, throwCount));
        }
        else
        {
            Debug.LogError("Is not Server");
        }
    }

    private IEnumerator ThrowDiceCoroutine(int dice1, int dice2, int throwCount)
    {
        for (int i = 0; i < throwCount; i++)
        {
            this.dice1.ThrowDiceServerRpc(dice1);
            this.dice2.ThrowDiceServerRpc(dice2);
            yield return new WaitForSeconds(3);//Animation finish
        }
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(player.Value, out NetworkClient client))
        {
            client.PlayerObject.GetComponent<Player>().CalculateTargetAndMove((dice1 + dice2)*throwCount);
        }
        _playerStatus.Value = PlayerStatus.PlayerIsPlayed;

        if ((int)(playOrder.Value + 1) < NetworkManager.Singleton.ConnectedClients.Count)
        {
            playOrder.Value++;
        }
        else
        {
            playOrder.Value = 0;
        }
        player.Value = NetworkManager.Singleton.ConnectedClientsIds[playOrder.Value];
        _playerStatus.Value = PlayerStatus.PlayerIsNotPlaying;
        //The new player can now roll dice. It tells the player that it is possible to roll the dice to declare this.
        SendPlayerOrderStatusClientRpc(true, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player.Value }
            }
        });
    }

    [ClientRpc(AllowTargetOverride = true)]
    private void SendPlayerOrderStatusClientRpc(bool status, ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(SendPlayerOrderStatus(status));

    }
    private IEnumerator SendPlayerOrderStatus(bool status)
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

    private IEnumerator SetFirstPlayerOrder()
    {
        yield return new WaitForSeconds(2);
        player.Value = NetworkManager.Singleton.ConnectedClientsIds[0];
        playOrder.Value = 0;
        SendPlayerOrderStatusClientRpc(true, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { player.Value }
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