using Unity.Entities;
using UnityEngine;
using Unity.Collections;

public struct SpawnerData : IComponentData
{
    public Entity jamGameEntity;
    public double spawnRate;
    public double lastSpawnTime;
}
