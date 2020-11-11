using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct Paddle : IComponentData {
  [GhostField] public float Speed;
  [GhostField] public float3 Dimensions;
}

public class PaddleAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public float Speed;
  public float3 Dimensions;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new Paddle {
      Speed = Speed,
      Dimensions = Dimensions
    });
  }

  public void OnDrawGizmos() {
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(transform.position, Dimensions);
  }
}