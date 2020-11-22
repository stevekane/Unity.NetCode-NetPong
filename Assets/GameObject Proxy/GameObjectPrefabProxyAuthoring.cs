using Unity.Entities;
using UnityEngine;

public struct GameObjectPrefabProxy : IComponentData {
  public int ID;
}

public class GameObjectPrefabProxyAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public GameObject Prefab;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new GameObjectPrefabProxy {
      ID = Prefab.Hash()
    });
  }
}