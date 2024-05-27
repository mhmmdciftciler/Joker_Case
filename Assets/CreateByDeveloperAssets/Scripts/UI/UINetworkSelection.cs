using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UINetworkSelection : MonoBehaviour
{
    [SerializeField] NetworkManager _networkManager;
    [SerializeField] GameNetworkManager _gameNetwork;
    [SerializeField] TileManager _tanager;
    [SerializeField] GameObject _SelectServerTypeMenu;
    [SerializeField] TMP_InputField _dice1;
    [SerializeField] TMP_InputField _dice2;
    [SerializeField] TMP_InputField _throwCount;//Plus
    void Start()
    {
        _networkManager.OnClientConnectedCallback += OnConnect;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnConnect(ulong a)
    {
        _SelectServerTypeMenu.SetActive(false);
    }
    public void SelectHost()
    {
        _networkManager.StartHost();
    }
    public void SelectClient()
    {
        _networkManager.StartClient();
    }
    public void DiceSet()
    {
        int diceCount1 = int.Parse(_dice1.text);
        int diceCount2 = int.Parse(_dice2.text);
        int throwCount = int.Parse(_throwCount.text);
        if (diceCount1 > 6 || diceCount1 <= 0 || diceCount2 > 6 || diceCount2 <= 0 || throwCount < 1)//Game Rule
        {
            CaseLogger.Instance.Logger("Dice values and throw count cannot be less than 1. dice values cannot be greater than 6.", Color.red);
            return;
        }
        _gameNetwork.ThrowDiceServerRpc(_networkManager.LocalClientId, diceCount1, diceCount2, throwCount);
    }
}
