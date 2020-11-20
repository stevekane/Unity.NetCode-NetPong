using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct GameObjectCacheBlobAsset {
  public BlobArray<GameObjectPair> GameObjectPairs;
}

public struct GameObjectPair {
  public int Id;
  public GameObject GameObject;

  public GameObjectPair(int id, GameObject gameObject) {
    Id = id;
    GameObject = gameObject;
  }
}

public struct GameObjectCache : IComponentData {
  public BlobAssetReference<GameObjectCacheBlobAsset> Reference;

  public static bool TryGet(GameObjectCache cache, int index, out GameObject gameObject) {
    for (int i = 0; i < cache.Reference.Value.GameObjectPairs.Length; i++) {
      if (cache.Reference.Value.GameObjectPairs[i].Id == index) {
        gameObject = cache.Reference.Value.GameObjectPairs[i].GameObject;
        return true;
      }
    }
    gameObject = null;
    return false;
  }
}

public class GameObjectCacheAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public string PathFromResourcesRoot = "";

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    var gameObjects = Resources.LoadAll<GameObject>(PathFromResourcesRoot);
    BlobAssetReference<GameObjectCacheBlobAsset> reference;

    using (var builder = new BlobBuilder(Allocator.Temp)) {
      ref var root = ref builder.ConstructRoot<GameObjectCacheBlobAsset>();
      var clipPairs = builder.Allocate(ref root.GameObjectPairs, gameObjects.Length);

      for (int i = 0; i < gameObjects.Length; i++) {
        Debug.Log($"Adding Prefab {gameObjects[i].name} with ID {gameObjects[i].GetInstanceID()} to GameObjectCache.");
        clipPairs[i] = new GameObjectPair(gameObjects[i].GetInstanceID(), gameObjects[i]);
      }

      reference = builder.CreateBlobAssetReference<GameObjectCacheBlobAsset>(Allocator.Persistent);
      Debug.Log($"{clipPairs.Length} total items in cache");
    }
    dstManager.AddComponentData(entity, new GameObjectCache { Reference = reference });
  }
}