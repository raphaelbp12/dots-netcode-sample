using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GenerateAuthoringComponent]
public struct LifeTimeComponent : IComponentData
{
    [GhostDefaultField(quantizationFactor: 1000, interpolate: true)]
    public float Value;
}
