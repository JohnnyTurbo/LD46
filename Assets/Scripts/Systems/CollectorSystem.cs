using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;

public class CollectorSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent();
        EntityQuery entityQuery = GetEntityQuery(typeof(JamGameData), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> translations = entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<JamGameData> jamGameDatas = entityQuery.ToComponentDataArray<JamGameData>(Allocator.TempJob);
        Entities.ForEach((Entity entity, int entityInQueryIndex, in CollectorData cData, in Translation position, in NonUniformScale scale) =>
        {
            if (cData.canCollect)
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity ent = entities[i];
                    float3 entPos = translations[i].Value;
                    if (math.distance(entPos, position.Value) <= (scale.Value.x / 2))
                    {
                        if(jamGameDatas[i].isAttracted == false)
                        {
                            commandBuffer.SetComponent(entityInQueryIndex, ent, new JamGameData
                            {
                                isAttracted = true,
                                attractorID = cData.attractorID,
                                curPosIndex = 0
                            });
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    if(jamGameDatas[i].isAttracted && jamGameDatas[i].attractorID == cData.attractorID)
                    {
                        Entity ent = entities[i];
                        commandBuffer.SetComponent(entityInQueryIndex, ent, new JamGameData
                        {
                            isAttracted = false,
                            attractorID = -1,
                            curPosIndex = 0
                        });
                    }
                }
            }
        }).Schedule();
        ecbSystem.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
        
        entities.Dispose();
        translations.Dispose();
        jamGameDatas.Dispose();
    }

}
