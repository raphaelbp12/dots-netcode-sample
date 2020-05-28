using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;

#if true

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class MoveCubeSystem : ComponentSystem
{

    Quaternion newRotation = new Quaternion();
    protected override void OnUpdate()
    {
        var raycaster = new MouseRayCast() { pw = World.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld };

        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((DynamicBuffer<CubeInput> inputBuffer, ref Translation trans, ref PredictedGhostComponent prediction, ref Rotation rot) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            CubeInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            Unity.Physics.RaycastHit? movDestination = raycaster.CheckRay(input.mousePosition, trans.Value, 1000f);
            if (movDestination != null)
            {
                Vector3 relativePos = movDestination.Value.Position - trans.Value;
                Vector3 eulerAngles = Quaternion.LookRotation(relativePos, Vector3.up).eulerAngles;

                float3 oldRotation = rot.Value.value.xyz;
                newRotation = Quaternion.Euler(oldRotation.x, eulerAngles.y, oldRotation.z);
            }
            rot.Value = newRotation;

            if (input.horizontal > 0)
                trans.Value.x += deltaTime;
            if (input.horizontal < 0)
                trans.Value.x -= deltaTime;
            if (input.vertical > 0)
                trans.Value.z += deltaTime;
            if (input.vertical < 0)
                trans.Value.z -= deltaTime;
        });
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