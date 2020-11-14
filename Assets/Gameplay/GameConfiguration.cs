using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameConfiguration : IComponentData {
  public float PaddleSpanRadians;
  public float PaddleSpeed;
  public float BallSpeed;
  public float ArenaRadius;
}