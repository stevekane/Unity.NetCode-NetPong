using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct GameObjectReference {
  public GameObject GameObject;
}

public struct ClientOnlyGameObjectPrefabs : IComponentData {
  public BlobAssetReference<GameObjectReference> ScoreGUIPrefabReference;
}

public class ClientOnlyGameObjectPrefabsAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public static BlobAssetReference<GameObjectReference> BuildGameObjectReference(GameObject go) {
    using (var builder = new BlobBuilder(Allocator.Temp)) {
      ref GameObjectReference gof = ref builder.ConstructRoot<GameObjectReference>();

      gof.GameObject = go;
      return builder.CreateBlobAssetReference<GameObjectReference>(Allocator.Persistent);
    }
  }

  public GameObject ScoreGUIPrefab;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new ClientOnlyGameObjectPrefabs {
      ScoreGUIPrefabReference = BuildGameObjectReference(ScoreGUIPrefab)
    });
  }
}