using UnityEngine;

public class UIInventory : UIBaseItemList<UIInventoryItem,UIInventoryItemData>
{
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
