using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public List<StageSettings> Stages;
    public int InitialLives = 3;
    public WeaponTypeToAssetReferenceKey WeaponsDict;
    public TargetTypeToAssetReferenceKey TargetsDict;
}

[Serializable]
public class WeaponTypeToAssetReferenceKey : UnitySerializedDictionary<WeaponType, string> {}

[Serializable]
public class TargetTypeToAssetReferenceKey : UnitySerializedDictionary<TargetType, string> {}
