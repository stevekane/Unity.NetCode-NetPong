using Unity.Collections;
using Unity.Scenes;
using Unity.Entities;
using Unity.Jobs;

public struct SubSceneLoadRequest : IComponentData {
  public Hash128 SceneHash;
}

public struct SubSceneUnloadRequest : IComponentData {
  public Hash128 SceneHash;
}

[AlwaysUpdateSystem]
[UpdateInGroup(typeof(SceneSystemGroup))]
[UpdateBefore(typeof(SceneSystem))]
public class SubSceneRequestSystem : SystemBase {
  SceneSystem SceneSystem;

  public static Entity CreateSubSceneLoadRequest(EntityManager entityManager, Hash128 sceneGUID) {
    var loadRequestEntity = entityManager.CreateEntity(typeof(SubSceneLoadRequest));
    var subSceneLoadRequest = new SubSceneLoadRequest { SceneHash = sceneGUID };

    entityManager.SetComponentData(loadRequestEntity, subSceneLoadRequest);
    return loadRequestEntity;
  }

  public static Entity CreateSubSceneUnloadRequest(EntityManager entityManager, Hash128 sceneGUID) {
    var unloadRequestEntity = entityManager.CreateEntity(typeof(SubSceneUnloadRequest));
    var subSceneLoadRequest = new SubSceneUnloadRequest { SceneHash = sceneGUID };

    entityManager.SetComponentData(unloadRequestEntity, subSceneLoadRequest);
    return unloadRequestEntity;
  }

  protected override void OnCreate() {
    SceneSystem = World.GetExistingSystem<SceneSystem>();
  }

  protected override void OnUpdate() {
    Entities
    .ForEach((Entity e, ref SubSceneLoadRequest loadRequest) => {
      var loadParameters = new SceneSystem.LoadParameters {
        Flags = SceneLoadFlags.LoadAdditive
      };

      SceneSystem.LoadSceneAsync(loadRequest.SceneHash, loadParameters);
      EntityManager.DestroyEntity(e);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();
    Entities

    .ForEach((Entity e, ref SubSceneUnloadRequest unloadRequest) => {
      SceneSystem.UnloadScene(unloadRequest.SceneHash);
      EntityManager.DestroyEntity(e);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();
  }
}