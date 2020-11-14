using Unity.Entities;

[GenerateAuthoringComponent]
public struct LifeCycle : IComponentData {
  public enum State { Alive, Dead }

  public State CurrentState;
}