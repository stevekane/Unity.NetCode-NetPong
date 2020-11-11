using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

public struct SubSceneReferences : IComponentData {
  public Unity.Entities.Hash128 GhostPrefabs;
  public Unity.Entities.Hash128 Board;
}

public class SubSceneReferencesAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public SubScene GhostPrefabs;
  public SubScene Board;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new SubSceneReferences {
      GhostPrefabs = GhostPrefabs.SceneGUID,
      Board = Board.SceneGUID
    });
  }
}