using Unity.Entities;

[GenerateAuthoringComponent]
public struct TeamOwner : IComponentData {
  public int TeamIndex; // -1 none
}