using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct Paddle : IComponentData {
  [GhostField] public float Radians;
}