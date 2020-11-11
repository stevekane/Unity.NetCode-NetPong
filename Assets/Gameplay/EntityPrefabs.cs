using Unity.Entities;

[GenerateAuthoringComponent]
public struct EntityPrefabs : IComponentData {
  public Entity Paddle;
  public Entity Ball;
}