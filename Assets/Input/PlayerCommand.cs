using Unity.NetCode;

public struct PlayerCommand : ICommandData {
  public readonly static byte Up = 1;
  public readonly static byte Down = 2;

  public uint Tick { get; set; }
  public byte Keys;

  public PlayerCommand(uint tick, float vertical) {
    Tick = tick;
    Keys = 0;

    if (vertical > 0) {
      Keys |= Up;
    }
    if (vertical < 0) {
      Keys |= Down;
    }
  }

  public bool Pushed(in byte b) {
    return (Keys & b) == b;
  }
}