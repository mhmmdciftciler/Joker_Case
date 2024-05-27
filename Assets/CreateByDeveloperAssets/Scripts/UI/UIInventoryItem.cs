using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventoryItem : UIBaseItem
{
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Animator _animator;
    [HideInInspector] public UIInventoryItemData ItemData;
    public override void Initialize(UIBaseItemData data)
    {
        ItemData = (UIInventoryItemData)data;
        Debug.Assert(ItemData != null, "data can not convertable!");
        Icon.sprite = ItemData.Icon;
        Name.text = ItemData.Name;
        _countText.text = ItemData.Count.ToString();
        _animator.Play("Collect");
    }

}
public class UIInventoryItemData : UIBaseItemData
{
    public int Count;
    public UIInventoryItemData(FruitData data, int count)
    {
        Icon = data.Icon;
        Name = data.Name;
        Count = count;
    }
}