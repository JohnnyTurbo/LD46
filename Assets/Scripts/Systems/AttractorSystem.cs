using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

public class AttractorSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent();
        float deltaTime = Time.DeltaTime;
        List<float3[]> allPaths = InputController.instance.movePaths;
        int arrayLength = allPaths.Count * allPaths[0].Length;
        NativeArray<float3> pathList = new NativeArray<float3>(arrayLength, Allocator.TempJob);

        int i = 0;
        for (int y = 0; y < allPaths.Count; y++)
        {
            for(int x = 0; x < allPaths[y].Length; x++)
            {
                pathList[i] = allPaths[y][x];
                i++;
            }
            //Debug.Log("array y: " + y + " has length of " + allPaths[y].Length);
        }

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation position, ref JamGameData jamGameData, in JamTravelData jamTravelData) =>
        {
            if (jamGameData.isAttracted)
            {
                float3 curTarget = pathList[jamGameData.curPosIndex];

                position.Value += deltaTime * jamTravelData.speed * (curTarget - position.Value);
                if (math.distance(position.Value, curTarget) < 1f)
                {
                    jamGameData.curPosIndex++;
                    if (jamGameData.curPosIndex >= pathList.Length)
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new DeleteTag());
                        //jamGameData.curPosIndex = 0;
                        //commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                        //ServerController.instance.IncreaseServerLoad(1);
                        //numDestroyed++;
                    }
                }
            }
            else
            {
                position.Value += deltaTime * jamTravelData.speed * (jamTravelData.startPos - position.Value);
            }
        }).ScheduleParallel();
        Dependency.Complete();
        pathList.Dispose();
        //ServerController.instance.IncreaseServerLoad(numDestroyed);
    }
}
