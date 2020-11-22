using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct AudioClipCacheBlobAsset {
  public BlobArray<AudioClipPair> AudioClipPairs;
}

public struct AudioClipPair {
  public int Id;
  public AudioClip AudioClip;

  public AudioClipPair(int id, AudioClip audioClip) {
    Id = id;
    AudioClip = audioClip;
  }
}

public struct AudioClipCache : IComponentData {
  public BlobAssetReference<AudioClipCacheBlobAsset> Reference;

  public AudioClip this[int index] {
    get {
      for (int i = 0; i < Reference.Value.AudioClipPairs.Length; i++) {
        if (Reference.Value.AudioClipPairs[i].Id == index) {
          return Reference.Value.AudioClipPairs[i].AudioClip;
        }
      }
      return null;
    }
  }

  public static AudioClipCache FromResources(AudioClip[] gameObjects) {
    BlobAssetReference<AudioClipCacheBlobAsset> reference;

    using (var builder = new BlobBuilder(Allocator.Temp)) {
      ref var root = ref builder.ConstructRoot<AudioClipCacheBlobAsset>();
      var clipPairs = builder.Allocate(ref root.AudioClipPairs, gameObjects.Length);

      for (int i = 0; i < gameObjects.Length; i++) {
        Debug.Log($"Adding Prefab {gameObjects[i].name} with ID {gameObjects[i].GetInstanceID()} to AudioClipCache.");
        clipPairs[i] = new AudioClipPair(gameObjects[i].Hash(), gameObjects[i]);
      }
      reference = builder.CreateBlobAssetReference<AudioClipCacheBlobAsset>(Allocator.Persistent);
      Debug.Log($"{clipPairs.Length} total items in cache");
    }
    return new AudioClipCache { Reference = reference };
  }
}