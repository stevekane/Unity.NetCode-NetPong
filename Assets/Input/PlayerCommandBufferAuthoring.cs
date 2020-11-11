using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerCommandBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddBuffer<PlayerCommand>(entity);
  }
}