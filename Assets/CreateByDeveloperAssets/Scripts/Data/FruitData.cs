using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Fruit Data", menuName = "CaseObject/FruitData", order = 0)]
public class FruitData : ScriptableObject
{
    public string Name;
    public string Description;
    public int Value;//Meyvelerin birbirinden farklý parametreleri olsun istedim. Silinebilir.
    public Sprite Icon;
    public PoolObject CollectPrefab;
}
