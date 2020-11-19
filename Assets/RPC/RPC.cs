using Unity.Entities;
using Unity.NetCode;

public struct RpcRequestLevel : IRpcCommand {}
public struct RpcsRequestLevelAck : IRpcCommand {
  public Hash128 LevelGUID;
}
public struct RpcJoinGame : IRpcCommand {}
public struct RpcJoinGameAck : IRpcCommand {}
public struct RpcLeaveGame : IRpcCommand {}
public struct RpcLeaveGameAck : IRpcCommand {}