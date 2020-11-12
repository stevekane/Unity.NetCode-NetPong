using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Bounds : IComponentData {
  public float3 Min;
  public float3 Max;
  public float Radius;
}

public class BoundsAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public float3 Min;
  public float3 Max;
  public float Radius;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new Bounds {
      Min = Min,
      Max = Max,
      Radius = Radius
    });
  }

  public void OnDrawGizmos() {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(new float3(0,0,0), Radius);
  }
}