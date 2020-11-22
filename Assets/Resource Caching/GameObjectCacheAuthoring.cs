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

  public bool TryGet(int index, out GameObject gameObject) {
    for (int i = 0; i < Reference.Value.GameObjectPairs.Length; i++) {
      if (Reference.Value.GameObjectPairs[i].Id == index) {
        gameObject = Reference.Value.GameObjectPairs[i].GameObject;
        return true;
      }
    }
    gameObject = null;
    return false;
  }

  public static GameObjectCache FromResources(GameObject[] gameObjects) {
    BlobAssetReference<GameObjectCacheBlobAsset> reference;

    using (var builder = new BlobBuilder(Allocator.Temp)) {
      ref var root = ref builder.ConstructRoot<GameObjectCacheBlobAsset>();
      var clipPairs = builder.Allocate(ref root.GameObjectPairs, gameObjects.Length);

      for (int i = 0; i < gameObjects.Length; i++) {
        Debug.Log($"Adding Prefab {gameObjects[i].name} with ID {gameObjects[i].Hash()} to GameObjectCache.");
        clipPairs[i] = new GameObjectPair(gameObjects[i].Hash(), gameObjects[i]);
      }
      reference = builder.CreateBlobAssetReference<GameObjectCacheBlobAsset>(Allocator.Persistent);
      Debug.Log($"{clipPairs.Length} total items in cache");
    }
    return new GameObjectCache { Reference = reference };
  }
}