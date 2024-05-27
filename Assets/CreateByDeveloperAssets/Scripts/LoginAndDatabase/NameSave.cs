using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameSave : MonoBehaviour
{
    [SerializeField] TMP_InputField _username;
    [SerializeField] GameObject _playerNameArea;

    [SerializeField] UIAvatarList uIAvatarList;
    public void SavePlayerName()//ButtonEvent
    {
        if (_username.text.Length > 3)
        {
            DatabaseManager.Instance.PlayerData.PlayerName = _username.text;
            DatabaseManager.Instance.SavePlayerData();
            _playerNameArea.SetActive(false);
            uIAvatarList.gameObject.SetActive(true);
            uIAvatarList.Init();
        }
        else
        {
            CaseLogger.Instance.Logger("Avatar name too short!", Color.red);
        }
    }


}
