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
}

public class AudioClipCacheAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public string PathFromResourcesRoot = "";

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    var audioClips = Resources.LoadAll<AudioClip>(PathFromResourcesRoot);

    using (var builder = new BlobBuilder(Allocator.Temp)) {
      ref var root = ref builder.ConstructRoot<AudioClipCacheBlobAsset>();
      var clipPairs = builder.Allocate(ref root.AudioClipPairs, audioClips.Length);

      for (int i = 0; i < audioClips.Length; i++) {
        Debug.Log($"Adding {audioClips[i].name} with ID {audioClips[i].GetInstanceID()} to AudioClipCache.");
        clipPairs[i] = new AudioClipPair(audioClips[i].GetInstanceID(), audioClips[i]);
      }

      var reference = builder.CreateBlobAssetReference<AudioClipCacheBlobAsset>(Allocator.Persistent);

      dstManager.AddComponentData(entity, new AudioClipCache { Reference = reference });
    }
  }
}