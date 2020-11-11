using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct Scores : IComponentData {
  [GhostField] public ushort LeftTeam;
  [GhostField] public ushort RightTeam;
}