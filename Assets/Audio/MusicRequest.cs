using Unity.Entities;

public struct MusicRequest : IComponentData {
  public uint Priority;
  public float Volume;
}