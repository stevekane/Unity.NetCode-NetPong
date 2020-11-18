using Unity.Entities;
using Unity.NetCode;

public struct RpcJoinGame : IRpcCommand {}
public struct RpcJoinGameAck : IRpcCommand {}
public struct RpcsLoadSubScene : IRpcCommand {
  public Hash128 SceneGUID;
}
public struct RpcLeaveGame : IRpcCommand {}
public struct RpcLeaveGameAck : IRpcCommand {}