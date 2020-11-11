using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostComponent(PrefabType=GhostPrefabType.AllPredicted)]
public struct Claimed : IComponentData {
  [GhostField] public ushort TeamIndex;
}