using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Bounds : IComponentData {
  public float3 Min;
  public float3 Max;
}

public class BoundsAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public float3 Min;
  public float3 Max;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new Bounds {
      Min = Min,
      Max = Max
    });
  }

  public void OnDrawGizmos() {
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(new float3(0,0,0), Max - Min);
  }
}