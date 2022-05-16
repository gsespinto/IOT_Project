using System;
using UnityEngine;

[Serializable]
public struct FBateryColor
{
    [Range(0, 1)] public float bateryPercentage;
    public Color color;
}