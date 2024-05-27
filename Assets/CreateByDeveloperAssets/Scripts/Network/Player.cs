using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class Player : NetworkBehaviour
{
    [SerializeField] private List<GameAvatar> _gameAvatars;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private TextMeshPro _playerNameText;
    private Animator _animator;
    private bool isMoving = false;
    private TileManager _tileManager;
    private UIInventory _inventory;
    public NetworkVariable<int> AvatarIndex = new NetworkVariable<int>();
    public NetworkVariable<int> TileIndex = new NetworkVariable<int>();
    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>();
    private Vector3 _targetPosition;
    private Vector3 _firstForward;
    private Vector3 _calculatedTarget;
    private int _targetTile;
    private Vector3 _camera;
    private PlayerData _playerData;
    NetworkObjectHandler _networkObjectHandler;
    NetworkTransform _transform;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _networkObjectHandler = NetworkManager.gameObject.GetComponent<NetworkObjectHandler>();
        _tileManager = _networkObjectHandler.TileManager;

        if (IsOwner)
        {
            _playerData = DatabaseManager.Instance.PlayerData;
            _inventory = _networkObjectHandler.Inventory;
            PlayerSyncServerRpc(DatabaseManager.Instance.PlayerData.PlayerName,
                DatabaseManager.Instance.PlayerData.AvatarIndex);
            _playerNameText.text = DatabaseManager.Instance.PlayerData.PlayerName;
        }
        _transform = GetComponent<NetworkTransform>();
        StartCoroutine(NetworkSpawn());
        _camera = Camera.main.transform.position;
    }

    private void Update()
    {

        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }
    IEnumerator NetworkSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        if (IsServer)
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
            TeleportToTileClientRpc(TileIndex.Value);
            SpawnAvatarClientRpc(AvatarIndex.Value);
        }
        if (_animator == null)
        {
            GameAvatar gameAvatar = Instantiate(_gameAvatars[AvatarIndex.Value], transform);
            gameAvatar.transform.localEulerAngles = Vector3.zero;
            gameAvatar.transform.localPosition = Vector3.zero;
            _animator = gameAvatar.animator;
            Tile tileObject = _tileManager.tiles[TileIndex.Value];
            transform.position = tileObject.transform.position;
            _playerNameText.text = PlayerName.Value.ToString();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void CalculateTargetAndMoveServerRpc(int diceValue)
    {
        _tileManager.tiles[_targetTile].SetPlayerOnServerRpc(false);
        _tileManager.tiles[_targetTile].SendSpawnFruitCallServerRpc();
        _targetTile = diceValue + this.TileIndex.Value;
        if (_targetTile >= _tileManager.tiles.Length)
        {
            _calculatedTarget = _tileManager.tiles[_tileManager.tiles.Length - 1].transform.position;
            _targetTile = diceValue + this.TileIndex.Value - _tileManager.tiles.Length;
            _targetPosition = _tileManager.tiles[_targetTile].transform.position;
        }
        else if (_targetTile < _tileManager.tiles.Length)
        {
            _calculatedTarget = _tileManager.tiles[_targetTile].transform.position;
            _targetPosition = _tileManager.tiles[_targetTile].transform.position;
        }
        Debug.Log("targetTile : " + _targetTile);
        isMoving = true;
        UpdateRunAnimationClientRpc(true);
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, _calculatedTarget, _moveSpeed * Time.deltaTime);
        _transform.Interpolate = true;
        float a = Vector3.Distance(transform.position, _calculatedTarget);
        if (a < 0.5f)
        {
            _firstForward = new Vector3(_camera.x - transform.position.x, 0, _camera.z - transform.position.z);
            transform.forward = Vector3.MoveTowards(transform.forward, _firstForward, 1 - a);
        }
        else
        {
            transform.forward = Vector3.MoveTowards(transform.forward, (_calculatedTarget - transform.position).normalized, .5f);
        }
        if (a < 0.01f)
        {

            if (_calculatedTarget == _targetPosition)
            {
                isMoving = false;
                UpdateRunAnimationClientRpc(false);
                TileIndex.Value = _targetTile;
                _tileManager.tiles[_targetTile].SetPlayerOnServerRpc(true);
                //if(IsLocalPlayer)
                //{
                //    DatabaseManager.Instance.SaveTransformData(_targetTile);//not Recommended this game 
                //}
            }

            if (IsOnTile(_tileManager.tiles.Length - 1) && _calculatedTarget != _targetPosition)
            {
                _calculatedTarget = _targetPosition;
                TeleportToTileServerRpc(0);
            }

        }

    }

    private bool IsOnTile(int tileIndex)
    {
        // Tile objelerini bir dizi olarak kabul ediyoruz
        Tile tileObject = _tileManager.tiles[tileIndex];
        return Vector3.Distance(transform.position, tileObject.transform.position) < 0.1f;
    }
    [ClientRpc]
    private void SpawnAvatarClientRpc(int avatarIndex)
    {
        Debug.Log("avatarIndex : " + avatarIndex);
        GameAvatar gameAvatar = Instantiate(_gameAvatars[avatarIndex], transform);
        gameAvatar.transform.localEulerAngles = Vector3.zero;
        gameAvatar.transform.localPosition = Vector3.zero;
        _animator = gameAvatar.animator;
        _playerNameText.text = PlayerName.Value.ToString();
        if (!IsLocalPlayer)
            return;
        _inventory.SetPlayerName(DatabaseManager.Instance.PlayerData.PlayerName, gameAvatar.avatarData.Icon);
        for (int i = 0; i < DatabaseManager.Instance.PlayerData.InventoryDatas.Length; i++)
        {
            FruitData data = DatabaseManager.Instance.GetData(DatabaseManager.Instance.PlayerData.InventoryDatas[i].DataName);
            _inventory.CreateOrSet(data, DatabaseManager.Instance.PlayerData.InventoryDatas[i].DataValue);
        }


    }
    [ServerRpc(RequireOwnership = false)]
    private void TeleportToTileServerRpc(int tileIndex)
    {
        TeleportToTileClientRpc(tileIndex);
    }

    [ClientRpc]
    private void TeleportToTileClientRpc(int tileIndex)
    {
        if (IsOwner && _transform)
            _transform.Interpolate = false;
        Tile tileObject = _tileManager.tiles[tileIndex];
        transform.position = tileObject.transform.position;
        tileObject.SetPlayerOnServerRpc(true);
    }

    [ClientRpc]
    private void UpdateRunAnimationClientRpc(bool isRunning)
    {
        _animator.SetBool("Run", isRunning);
    }
    [ServerRpc(RequireOwnership = false)]
    public void CollectFruitServerRpc(ulong fruitNetworkObjectId, ulong playerID)
    {
        NetworkObject fruitNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[fruitNetworkObjectId];
        if (fruitNetworkObject != null)
        {
            StartCoroutine(DespawnFruit(fruitNetworkObject));
            FruitData fruitData = fruitNetworkObject.GetComponent<Fruit>().data;

            CollectFruitClientRpc(DatabaseManager.Instance.GetDataIndex(fruitData),
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerID }
                }
            });
        }
    }
    private IEnumerator DespawnFruit(NetworkObject fruit)
    {
        yield return new WaitForSeconds(1);
        fruit.Despawn();
    }
    [ClientRpc(AllowTargetOverride = true)]
    public void CollectFruitClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Client collect");
        _playerData.InventoryDatas[index].DataValue += 1;

        _inventory.CreateOrSet(DatabaseManager.Instance.GetData(_playerData.InventoryDatas[index].DataName),
            _playerData.InventoryDatas[index].DataValue);

        DatabaseManager.Instance.SaveInventoryData(index, _playerData.InventoryDatas[index]);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _tileManager.tiles[TileIndex.Value].SetPlayerOnServerRpc(false);
    }
    [ServerRpc(RequireOwnership = true)]
    private void PlayerSyncServerRpc(string name, int avatarIndex)
    {
        if (IsServer)
        {
            PlayerName.Value = name;
            AvatarIndex.Value = avatarIndex;
        }

    }
}
