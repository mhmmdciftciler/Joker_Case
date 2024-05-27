using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAvatarList : UIBaseItemList<UIAvatar, UIAvatarData>
{
    [SerializeField] List<GameAvatar> _characterPrefab;
    [SerializeField] Transform _characterContainer;
    [SerializeField] ToggleGroup _toggleGroup;
    int _avatarIndex;
    public override void SetItemData(UIAvatar uIAvatar, UIAvatarData data)
    {
        uIAvatar.Initialize(data);
        uIAvatar.GetComponent<Toggle>().group = _toggleGroup;
    }
    public void Init()
    {
        int avatarIndex = 0;
        List<UIAvatarData> uIAvatarDatas = new List<UIAvatarData>();
        foreach (var character in _characterPrefab)
        {
            GameAvatar avatar = Instantiate(character, _characterContainer);
            avatar.gameObject.SetActive(false);
            UIAvatarData data = new UIAvatarData();
            data.Avatar = avatar;
            data.AvatarIndex = avatarIndex;
            data.AvatarList = this;
            uIAvatarDatas.Add(data);
            avatarIndex++;
        }
        Generate(uIAvatarDatas);
    }
    public void SelectAvatar(int index)
    {
        _avatarIndex = index;
    }
    public void SaveAvatarIndex()
    {
        DatabaseManager.Instance.SaveAvatarData(_avatarIndex);
        CaseLogger.Instance.Logger("Saved to database did you selection.", _avatarIndex == 0 ? Color.cyan : Color.yellow);
    }
}
