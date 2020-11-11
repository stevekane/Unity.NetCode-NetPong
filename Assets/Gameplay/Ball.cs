using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct Ball : IComponentData {
  [GhostField] public float Speed;
}