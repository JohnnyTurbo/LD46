using Unity.Entities;

public class DeleteSystem : SystemBase
{
    //EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        //ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //var commandBuffer = ecbSystem.CreateCommandBuffer();
        Entities.ForEach((Entity entity, int entityInQueryIndex, in DeleteTag delTag) => 
        {
            //commandBuffer.DestroyEntity(entity);
            Spawner.instance.jamGames.Remove(entity);
            EntityManager.DestroyEntity(entity);
            ServerController.instance.IncreaseServerLoad(1);
        }).WithoutBurst().WithStructuralChanges().Run();
        //Dependency.Complete();
    }
}
