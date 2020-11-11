using Unity.Entities;
using UnityEngine;

using Random = Unity.Mathematics.Random;

public struct BallSpawner : IComponentData {
  public float SpawnsPerTick;
  public float TimeRemainder;
  public Random Random;
}

public class BallSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public float SpawnsPerTick;
  public uint Seed;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new BallSpawner {
      SpawnsPerTick = SpawnsPerTick,
      TimeRemainder = 0,
      Random = new Random(Seed)
    });
  }
}