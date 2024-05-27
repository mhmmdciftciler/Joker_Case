using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Fruit Data", menuName = "CaseObject/FruitData", order = 0)]
public class FruitData : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public PoolObject CollectPrefab;
}
