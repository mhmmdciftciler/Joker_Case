using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : UIBaseItemList<UIInventoryItem,UIInventoryItemData>
{
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private Image _playerIcon;
    public void SetPlayerName(string playerName, Sprite icon)//burada olmasa iyi olur, single responsive uygun deðil
    {
        _playerName.text = playerName;
        _playerIcon.sprite = icon;
    }
    public void CreateOrSet(FruitData data, int count)
    {
        bool setted = false;
        foreach (var item in generatedBaseItemList)
        {
            if(item.ItemData.Name == data.Name)
            {
                setted = true;
                item.ItemData.Count = count;
                item.Initialize(item.ItemData);
            }
        }
        if(!setted)
        {
            UIInventoryItem item = Instantiate(itemPrefabs[Random.Range(0,itemPrefabs.Count)],itemContainer);
            UIInventoryItemData itemData = new UIInventoryItemData(data, count);
            SetItemData(item, itemData);
            generatedBaseItemList.Add(item);
        }


    }
    public override void SetItemData(UIInventoryItem uIBaseItem, UIInventoryItemData uIInventoryItemData)
    {
        uIBaseItem.ItemData = uIInventoryItemData;
        uIBaseItem.Initialize(uIInventoryItemData);
    }
}
