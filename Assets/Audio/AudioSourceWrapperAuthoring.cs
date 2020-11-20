using Unity.Entities;
using UnityEngine;

public class AudioSourceWrapperAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
  public AudioSource AudioSource;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
    conversionSystem.AddHybridComponent(AudioSource);
  }
}