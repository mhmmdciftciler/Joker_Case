using UnityEngine;
public class UIAvatar : UIBaseItem
{
    [HideInInspector] public GameAvatar Avatar;
    private UIAvatarList _avatarList;
    private int _avatarIndex;

    public void Toggle(bool toggle)//Avatar Selected
    {
        Avatar.gameObject.SetActive(toggle);
        SelectedObject.SetActive(toggle);

        if (toggle && _avatarList != null)
        {
            _avatarList.SelectAvatar(_avatarIndex);
        }
    }

    public override void Initialize(UIBaseItemData data)
    {
        UIAvatarData uIavatarData = (UIAvatarData)data;
        uIavatarData.Icon = uIavatarData.Avatar.avatarData.Icon;
        uIavatarData.Name = uIavatarData.Avatar.avatarData.Name;
        _avatarList = uIavatarData.AvatarList;
        _avatarIndex = uIavatarData.AvatarIndex;
        Avatar = uIavatarData.Avatar;
        base.Initialize(data);
    }
}

[System.Serializable]
public class UIAvatarData : UIBaseItemData
{
    public GameAvatar Avatar;
    public int AvatarIndex;
    public UIAvatarList AvatarList;
}

