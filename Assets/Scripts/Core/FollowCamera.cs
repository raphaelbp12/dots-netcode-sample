using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class FollowCamera : SystemBase
{
    int angleIncrement = 1;
    int cameraAngle = 0;

    protected override void OnUpdate()
    {
        var tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        Entities.ForEach((DynamicBuffer<CubeInput> inputBuffer, ref Translation trans, ref Rotation rot) =>
        {
            CubeInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            Camera mainCamera;

            float heightOffset = 14;
            float zOffset = 8;

            if (input.toggleCameraZOffset == 1)
                zOffset = 3;

            if (input.cameraRotation != 0)
                cameraAngle += angleIncrement * input.cameraRotation;

            if (input.resetCameraRotation != 0)
                cameraAngle = 0;

            mainCamera = Camera.main;
            mainCamera.transform.rotation = Quaternion.Euler(60, cameraAngle, 0);

            mainCamera.transform.position = trans.Value + new float3(0, heightOffset, -zOffset);
        }).WithoutBurst().Run();
    }
}