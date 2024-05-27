using System.Collections;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference _databaseReference;
    public static DatabaseManager Instance;
    public UnityEvent OnLoadData;
    [HideInInspector] public PlayerData PlayerData { get; private set; }
    public List<FruitData> FruitDatas;
    IEnumerator Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        yield return new WaitUntil(() => FirebaseInitializer.Auth != null);
        _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        PlayerData = new PlayerData();
        PlayerData.InventoryDatas = new InventoryData[FruitDatas.Count];
        for (int i = 0; i < FruitDatas.Count; i++)
        {
            InventoryData data = new InventoryData();
            data.DataName = FruitDatas[i].Name;
            PlayerData.InventoryDatas[i] = data;
        }
    }
    public void SavePlayerData()
    {
        SavePlayerData(PlayerData);
    }
    private void SavePlayerData(PlayerData playerData)
    {
        string json = JsonUtility.ToJson(playerData);
        _databaseReference.Child("users").Child(FirebaseInitializer.User.UserId).SetRawJsonValueAsync(json);
    }
    public void SaveInventoryData(int index,InventoryData inventoryData)
    {
        string json = JsonUtility.ToJson(inventoryData);
        _databaseReference.Child("users").Child(FirebaseInitializer.User.UserId)
            .Child("InventoryDatas").Child(index.ToString()).SetRawJsonValueAsync(json);
    }
    public void SaveAvatarData(int avatarIndex)
    {
        _databaseReference.Child("users").Child(FirebaseInitializer.User.UserId).Child("AvatarIndex").SetValueAsync(avatarIndex);
        PlayerData.AvatarIndex = avatarIndex;
       
    }
    public void LoadData()
    {
        StartCoroutine(LoadDataCoroutine());
    }
    public int GetDataIndex(FruitData data)
    {
        return FruitDatas.IndexOf(data);
    }
    public FruitData GetData(string name)
    {
        for (int i = 0; i < FruitDatas.Count; i++)
        {
            if (FruitDatas[i].Name == name)
            {
                return FruitDatas[i];
            }
        }
        return null;
    }
    public IEnumerator LoadDataCoroutine()
    {
        Task<DataSnapshot> serverData = _databaseReference.Child("users").Child(FirebaseInitializer.User.UserId).GetValueAsync();
        
        yield return new WaitUntil(() => serverData.IsCompleted);
        
        string json = serverData.Result.GetRawJsonValue();
        
        if (json != null && json.Length>0)
        {
            PlayerData = JsonUtility.FromJson<PlayerData>(json);
            OnLoadData?.Invoke();
            Debug.Log(json);
            CaseLogger.Instance.Logger(json, Color.green);
        }
    }
}
[System.Serializable]
public class InventoryData
{
    public string DataName;
    public int DataValue;

}
[System.Serializable]
public class PlayerData
{
    public string PlayerName;
    public InventoryData[] InventoryDatas;
    public int AvatarIndex;
}