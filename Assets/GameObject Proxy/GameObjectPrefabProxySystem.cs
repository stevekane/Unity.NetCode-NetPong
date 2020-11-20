using Unity.Entities;
using UnityEngine;

public class GameObjectInstance : ISystemStateComponentData {
  public GameObject Instance;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class GameObjectPrefabProxySystem : SystemBase {
  protected override void OnCreate() {
    RequireSingletonForUpdate<GameObjectCache>();
  }

  protected override void OnUpdate() {
    var gameObjectCache = GetSingleton<GameObjectCache>();

    Entities
    .WithName("Create_Instance_For_PrefabProxy")
    .WithNone<GameObjectInstance>()
    .ForEach((Entity e, ref GameObjectPrefabProxy prefabProxy) => {
      if (GameObjectCache.TryGet(gameObjectCache, prefabProxy.ID, out GameObject gameObject)) {
        EntityManager.AddComponentData(e, new GameObjectInstance { Instance = GameObject.Instantiate(gameObject) });
      };
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();

    Entities
    .WithName("Destroy_Instance_For_PrefabProxy")
    .WithNone<GameObjectPrefabProxy>()
    .ForEach((Entity e, GameObjectInstance instance) => {
      GameObject.Destroy(instance.Instance);
      EntityManager.RemoveComponent<GameObjectInstance>(e);
    })
    .WithStructuralChanges()
    .WithoutBurst()
    .Run();
  }
}