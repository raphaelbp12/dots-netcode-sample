using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct MovableCubeComponent : IComponentData
{
    [GhostDefaultField]
    public int PlayerId;
}
