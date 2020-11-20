using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(ClientPresentationSystemGroup))]
public class ClientMusicControllerSystem : SystemBase {
  protected override void OnCreate() {
    RequireSingletonForUpdate<MusicAudioSource>();
    RequireSingletonForUpdate<AudioClipCache>();
  }

  protected override void OnUpdate() {
    var audioClipCache = GetSingleton<AudioClipCache>();
    var volume = Mathf.Abs(Mathf.Sin((float)Time.ElapsedTime / 10f));

    Entities
    .WithAll<MusicAudioSource>()
    .ForEach((Entity entity, GameObjectInstance instance) => {
      if (instance.Instance != null && instance.Instance.TryGetComponent(out AudioSource source)) {
        source.loop = true;
        source.volume = volume;
        if (!source.isPlaying) {
          source.Play();
        }
      }
    }) 
    .WithoutBurst()
    .Run();
  }
}