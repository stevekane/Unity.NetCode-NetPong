using Unity.Entities;

public struct ServerApplication : IComponentData {
  public enum State { Initialized, Loading, Running }

  public State CurrentState;
}