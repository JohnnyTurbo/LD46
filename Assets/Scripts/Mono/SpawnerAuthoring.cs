/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[RequiresEntityConversion]
public class SpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject jamGamePrefab;
    public double spawnRate;
    public Color[] colors;
    public char[] colChars;
    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(jamGamePrefab);
    }

    // Lets you convert the editor data representation to the entity optimal runtime representation
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new SpawnerData
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs.
            // So here we simply map the game object to an entity reference to that prefab.
            jamGameEntity = conversionSystem.GetPrimaryEntity(jamGamePrefab),
            spawnRate = spawnRate,
            lastSpawnTime = 0,
            colors = new NativeArray<Color>(colors, Allocator.Persistent),
            colChars = new NativeArray<char>(colChars, Allocator.Persistent)
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}
*/