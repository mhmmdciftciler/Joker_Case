
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBaseItem : MonoBehaviour, IUListItem
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public GameObject SelectedObject;

    public virtual void Initialize(UIBaseItemData data)
    {
        Icon.sprite = data.Icon;
        Name.text = data.Name;
    }
}
[System.Serializable]
public class UIBaseItemData
{
    public Sprite Icon;
    public string Name;
}