using Unity.Entities;

public class DeleteSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, int entityInQueryIndex, in DeleteTag delTag) => 
        {
            Spawner.instance.jamGames.Remove(entity);
            EntityManager.DestroyEntity(entity);
            ServerController.instance.IncreaseServerLoad(1);
        }).WithoutBurst().WithStructuralChanges().Run();
    }
}
