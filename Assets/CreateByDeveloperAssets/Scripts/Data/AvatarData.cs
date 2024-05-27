using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Avatar", menuName = "CaseObject/AvatarData", order = 1)]
public class AvatarData : ScriptableObject
{
    public string Name;
    public Sprite Icon;
}
