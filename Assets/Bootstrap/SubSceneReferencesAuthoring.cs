using Unity.Entities;
using UnityEngine;

public struct SubSceneReferences : IComponentData {
  public BoardSubSceneReferences DefaultBoardSubSceneReferences;
}

public class SubSceneReferencesAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public BoardSceneReferencesAuthoring BoardSubSceneReferences;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    dstManager.AddComponentData(entity, new SubSceneReferences {
      DefaultBoardSubSceneReferences = new BoardSubSceneReferences(BoardSubSceneReferences)
    });
  }
}