/*
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SpawnerSystem : SystemBase
{
    //EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    EntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        double curTime = Time.ElapsedTime;
        var commandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent();
        float3 rayStart = UnityEngine.Random.onUnitSphere * 55;
        float3 rayEnd = float3.zero - rayStart;
        Ray ray = new Ray(rayStart, rayEnd);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            
        }

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref SpawnerData spawnData) =>
        {
            //Debug.Log("Cur time: " + curTime.ToString() + " last spawn: " + spawnData.lastSpawnTime.ToString());
            if (spawnData.spawnRate <= (curTime - spawnData.lastSpawnTime))
            {
                spawnData.lastSpawnTime = curTime;
                
                //var instance = commandBuffer.Instantiate(entityInQueryIndex, spawnData.jamGameEntity);
                
                //var position = float3.zero;
                //commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = spawnPos });
            }
        }).Schedule();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
*/
