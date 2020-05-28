using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct dotsnetcodesampleGhostSerializerCollection : IGhostSerializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "CubeGhostSerializer",
            "CubeOnServerGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public static int FindGhostType<T>()
        where T : struct, ISnapshotData<T>
    {
        if (typeof(T) == typeof(CubeSnapshotData))
            return 0;
        if (typeof(T) == typeof(CubeOnServerSnapshotData))
            return 1;
        return -1;
    }

    public void BeginSerialize(ComponentSystemBase system)
    {
        m_CubeGhostSerializer.BeginSerialize(system);
        m_CubeOnServerGhostSerializer.BeginSerialize(system);
    }

    public int CalculateImportance(int serializer, ArchetypeChunk chunk)
    {
        switch (serializer)
        {
            case 0:
                return m_CubeGhostSerializer.CalculateImportance(chunk);
            case 1:
                return m_CubeOnServerGhostSerializer.CalculateImportance(chunk);
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int GetSnapshotSize(int serializer)
    {
        switch (serializer)
        {
            case 0:
                return m_CubeGhostSerializer.SnapshotSize;
            case 1:
                return m_CubeOnServerGhostSerializer.SnapshotSize;
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int Serialize(ref DataStreamWriter dataStream, SerializeData data)
    {
        switch (data.ghostType)
        {
            case 0:
            {
                return GhostSendSystem<dotsnetcodesampleGhostSerializerCollection>.InvokeSerialize<CubeGhostSerializer, CubeSnapshotData>(m_CubeGhostSerializer, ref dataStream, data);
            }
            case 1:
            {
                return GhostSendSystem<dotsnetcodesampleGhostSerializerCollection>.InvokeSerialize<CubeOnServerGhostSerializer, CubeOnServerSnapshotData>(m_CubeOnServerGhostSerializer, ref dataStream, data);
            }
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    private CubeGhostSerializer m_CubeGhostSerializer;
    private CubeOnServerGhostSerializer m_CubeOnServerGhostSerializer;
}

public struct EnabledotsnetcodesampleGhostSendSystemComponent : IComponentData
{}
public class dotsnetcodesampleGhostSendSystem : GhostSendSystem<dotsnetcodesampleGhostSerializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnabledotsnetcodesampleGhostSendSystemComponent>();
    }
}
