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
    private bool _isMoving = false;
    private TileManager _tileManager;
    private UIInventory _inventory;
    private UIPlayerInfo _uIPlayerInfo;
    public NetworkVariable<int> avatarIndex = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> tileIndex = new NetworkVariable<int>();//only can be write from server
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _targetPosition;
    private Vector3 _firstForward;
    private Vector3 _calculatedTarget;
    private int _targetTile;
    private Vector3 _camera;
    private PlayerData _playerData;
    private NetworkObjectHandler _networkObjectHandler;
    private NetworkTransform _transform;
    private GameAvatar _avatar;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _networkObjectHandler = NetworkManager.gameObject.GetComponent<NetworkObjectHandler>();
        _tileManager = _networkObjectHandler.TileManager;
        _uIPlayerInfo = _networkObjectHandler.UIPlayerInfo;

        if (IsOwner)
        {
            _playerData = DatabaseManager.Instance.PlayerData;
            _inventory = _networkObjectHandler.Inventory;
            _playerNameText.text = DatabaseManager.Instance.PlayerData.PlayerName;
            playerName.Value = DatabaseManager.Instance.PlayerData.PlayerName;
            avatarIndex.Value = DatabaseManager.Instance.PlayerData.AvatarIndex;
            SpawnAvatar();
            _uIPlayerInfo.SetPlayerName(DatabaseManager.Instance.PlayerData.PlayerName, _avatar.avatarData.Icon);
            for (int i = 0; i < DatabaseManager.Instance.PlayerData.InventoryDatas.Length; i++)
            {
                FruitData data = DatabaseManager.Instance.GetData(DatabaseManager.Instance.PlayerData.InventoryDatas[i].DataName);
                _inventory.CreateOrSet(data, DatabaseManager.Instance.PlayerData.InventoryDatas[i].DataValue);
            }
        }

        _transform = GetComponent<NetworkTransform>();
        StartCoroutine(NetworkSpawn());
        _camera = Camera.main.transform.position;
    }

    private void Update()
    {
        if (_isMoving)
        {
            MoveTowardsTarget();
        }
    }
    private IEnumerator NetworkSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        if (IsServer)
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
            TeleportToTileClientRpc(tileIndex.Value);
        }
        if(_avatar == null)
        {
            //Spawn clone avatar for other Clients
            SpawnAvatar();
            Tile tileObject = _tileManager.tiles[tileIndex.Value];
            transform.position = tileObject.transform.position;
        }
    }
    internal void CalculateTargetAndMove(int diceValue)
    {
        if (IsServer)
        {
            _tileManager.tiles[_targetTile].SetPlayerOnServerRpc(false);
            _tileManager.tiles[_targetTile].SendSpawnFruitCallServerRpc();
            _targetTile = diceValue + this.tileIndex.Value;
            if (_targetTile >= _tileManager.tiles.Length)
            {
                _calculatedTarget = _tileManager.tiles[_tileManager.tiles.Length - 1].transform.position;
                _targetTile = diceValue + this.tileIndex.Value - _tileManager.tiles.Length;
                _targetPosition = _tileManager.tiles[_targetTile].transform.position;
            }
            else if (_targetTile < _tileManager.tiles.Length)
            {
                _calculatedTarget = _tileManager.tiles[_targetTile].transform.position;
                _targetPosition = _tileManager.tiles[_targetTile].transform.position;
            }
            _isMoving = true;
            UpdateRunAnimationClientRpc(true);
        }

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
                _isMoving = false;
                UpdateRunAnimationClientRpc(false);
                tileIndex.Value = _targetTile;
                _tileManager.tiles[_targetTile].SetPlayerOnServerRpc(true);
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
        Tile tileObject = _tileManager.tiles[tileIndex];
        return Vector3.Distance(transform.position, tileObject.transform.position) < 0.1f;
    }
    private void SpawnAvatar()
    {
        _avatar = Instantiate(_gameAvatars[avatarIndex.Value], transform);
        _avatar.transform.localEulerAngles = Vector3.zero;
        _avatar.transform.localPosition = Vector3.zero;
        _animator = _avatar.animator;
        _playerNameText.text = playerName.Value.ToString();
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
        _playerData.InventoryDatas[index].DataValue += 1;

        _inventory.CreateOrSet(DatabaseManager.Instance.GetData(_playerData.InventoryDatas[index].DataName),
            _playerData.InventoryDatas[index].DataValue);

        DatabaseManager.Instance.SaveInventoryData(index, _playerData.InventoryDatas[index]);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _tileManager.tiles[tileIndex.Value].SetPlayerOnServerRpc(false);
    }
}
