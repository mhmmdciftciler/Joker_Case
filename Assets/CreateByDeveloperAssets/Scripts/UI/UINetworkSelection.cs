using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UINetworkSelection : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameNetworkManager gameNetwork;
    [SerializeField] TileManager tanager;
    [SerializeField] GameObject SelectServerTypeMenu;
    [SerializeField] TMP_InputField dice1;//The list must be added to increase the number of dice.
    [SerializeField] TMP_InputField dice2;
    void Start()
    {
        networkManager.OnClientConnectedCallback += OnConnect;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnConnect(ulong a)
    {
        SelectServerTypeMenu.SetActive(false);
    }
    public void SelectHost()
    {
        networkManager.StartHost();
    }
    public void SelectClient()
    {
        networkManager.StartClient();
    }
    public void DiceSet()//The list must be added to increase the number of dice.
    {
        gameNetwork.ThrowDiceServerRpc(networkManager.LocalClientId, int.Parse(dice1.text), int.Parse(dice2.text));
    }
}
