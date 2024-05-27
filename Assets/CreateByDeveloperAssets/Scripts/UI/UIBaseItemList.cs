using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The purpose of this class is to display a list with a general structure. It will be used for the Inventory and Character lists.
//"For this case, this code may not seem necessary.
//My intention is to demonstrate my knowledge. It might be very useful if there is a need to display many different types of lists."
public class UIBaseItemList<T,D> : MonoBehaviour where T : MonoBehaviour , IUListItem where D: UIBaseItemData
{
    public Transform itemContainer;
    public List<T> itemPrefabs;//Same type but hase diffirent visual prefabs for random selecet

    protected List<T> generatedBaseItemList = new();//Existing created list objects

    public void Generate(List<D> targetList)
    {
        while (generatedBaseItemList.Count < targetList.Count)//A loop that creates objects according to the amount of data in the target list
        {
            T uIBaseItem = Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Count)], itemContainer);
            generatedBaseItemList.Add(uIBaseItem);
        }
       
        for (int i = 0; i < generatedBaseItemList.Count; i++) // Update items and set active/inactive based on target list
        {
            if (i < targetList.Count)
            {
                SetItemData(generatedBaseItemList[i], targetList[i]);
                generatedBaseItemList[i].gameObject.SetActive(true);
            }
            else
            {
                generatedBaseItemList[i].gameObject.SetActive(false);
            }
        }
    }

    public virtual void SetItemData(T uIBaseItem, D data)
    {
        uIBaseItem.Initialize(data);
    }
}
public interface IUListItem
{
    void Initialize(UIBaseItemData uIBaseItemData);
}