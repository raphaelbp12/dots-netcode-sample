using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

#if true

#if SERVER_INPUT_SETUP
            var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
            var ghostId = GhostSerializerCollection.FindGhostType<CubeSnapshotData>();
            var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
            var player = EntityManager.Instantiate(prefab);
            EntityManager.SetComponentData(player, new MovableCubeComponent { PlayerId = EntityManager.GetComponentData<NetworkIdComponent>(req.SourceConnection).Value});

            PostUpdateCommands.AddBuffer<CubeInput>(player);
            PostUpdateCommands.SetComponent(req.SourceConnection, new CommandTargetComponent {targetEntity = player});
#endif

public struct CubeInput : ICommandData<CubeInput>
{
    public uint Tick => tick;
    public uint tick;
    public int horizontal;
    public int vertical;
    public float rotation;
    public int cameraRotation;
    public int resetCameraRotation;
    public int toggleFire;
    public int toggleCameraZOffset;
    public int fire;

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        horizontal = reader.ReadInt();
        vertical = reader.ReadInt();
        cameraRotation = reader.ReadInt();
        resetCameraRotation = reader.ReadInt();
        rotation = reader.ReadFloat();
        toggleFire = reader.ReadInt();
        toggleCameraZOffset = reader.ReadInt();
        fire = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(horizontal);
        writer.WriteInt(vertical);
        writer.WriteInt(cameraRotation);
        writer.WriteInt(resetCameraRotation);
        writer.WriteFloat(rotation);
        writer.WriteInt(toggleFire);
        writer.WriteInt(toggleCameraZOffset);
        writer.WriteInt(fire);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, CubeInput baseline,
        NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public void Serialize(ref DataStreamWriter writer, CubeInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class NetCubeSendCommandSystem : CommandSendSystem<CubeInput>
{
}
public class NetCubeReceiveCommandSystem : CommandReceiveSystem<CubeInput>
{
}

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SampleCubeInput : ComponentSystem
{
    CubeInput oldInput;
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnabledotsnetcodesampleGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate()
    {
        var raycaster = new MouseRayCast() { pw = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld };
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        var input = default(CubeInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
        if (localInput == Entity.Null)
        {
            Entities.WithNone<CubeInput>().ForEach((Entity ent, ref MovableCubeComponent cube) =>
            {
                if (cube.PlayerId == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<CubeInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }

        float3 mousePosition = Input.mousePosition;

        float3 trans = EntityManager.GetComponentData<Translation>(localInput).Value;

        Unity.Physics.RaycastHit? movDestination = raycaster.CheckRay(mousePosition, trans, 1000f);
        if (movDestination != null)
        {
            Vector3 relativePos = movDestination.Value.Position - trans;
            relativePos.y = 0;
            Vector3 eulerAngles = Quaternion.LookRotation(relativePos, Vector3.up).eulerAngles;
            input.rotation = eulerAngles.y;
        }

        input.toggleFire = oldInput.toggleFire;
        input.toggleCameraZOffset = oldInput.toggleCameraZOffset;

        if (Input.GetKey("a"))
            input.horizontal -= 1;
        if (Input.GetKey("d"))
            input.horizontal += 1;
        if (Input.GetKey("s"))
            input.vertical -= 1;
        if (Input.GetKey("w"))
            input.vertical += 1;
        if (Input.GetKey("q"))
            input.cameraRotation -= 1;
        if (Input.GetKey("e"))
            input.cameraRotation += 1;
        if (Input.GetKey("x"))
            input.resetCameraRotation = 1;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            input.toggleFire = input.toggleFire == 1 ? 0 : 1;
        if (Input.GetKeyDown("z"))
            input.toggleCameraZOffset = input.toggleCameraZOffset == 1 ? 0 : 1;
        if (Input.GetKey(KeyCode.Mouse0))
            input.fire = 1;
        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
        oldInput = input;
    }

    private struct MouseRayCast
    {
        public PhysicsWorld pw;
        public Unity.Physics.RaycastHit? CheckRay(float3 mousePosition, float3 playerPosition, float distance)
        {
            var cameraRay = Camera.main.ScreenPointToRay(mousePosition);
            Unity.Physics.RaycastHit hit;

            var ray = new RaycastInput()
            {
                Start = cameraRay.origin,
                End = cameraRay.origin + cameraRay.direction * distance,
                Filter = new CollisionFilter()
                {
                    BelongsTo = ~0u, // all 1s, so all layers, collide with everything 
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };
            bool hasHit = pw.CastRay(ray, out hit);

            if (hasHit)
            {
                //SetCursor(CursorType.Movement);
                return hit;
            }
            return null;
        }
    }
}
#endif