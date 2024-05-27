using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private Image _playerIcon;
    public void SetPlayerName(string playerName, Sprite icon)
    {
        _playerName.text = playerName;
        _playerIcon.sprite = icon;
    }
}
