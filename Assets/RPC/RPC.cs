using Unity.Entities;
using Unity.NetCode;

public struct RpcJoinGame : IRpcCommand {}
public struct RpcJoinGameAck : IRpcCommand {
  public Hash128 GhostsSubSceneGUID;
  public Hash128 BoardSubSceneGUID;
}
public struct RpcLeaveGame : IRpcCommand {}
public struct RpcLeaveGameAck : IRpcCommand {}