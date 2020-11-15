using System.Collections.Generic;
using Unity.NetCode;
using Unity.NetCode.Editor;
using static Unity.NetCode.Editor.GhostAuthoringComponentEditor;

public class GhostOverrides : IGhostDefaultOverridesModifier {
  public void Modify(Dictionary<string, GhostAuthoringComponentEditor.GhostComponent> overrides) {
    UnityEngine.Debug.Log("Disabling quantization of Translation and Rotation");
    overrides["Unity.Transforms.Translation"] = new GhostAuthoringComponentEditor.GhostComponent {
      name = "Unity.Transforms.Translation",
      attribute = new GhostComponentAttribute { PrefabType = GhostPrefabType.All, OwnerPredictedSendType = GhostSendType.All, SendDataForChildEntity = false },
      fields = new GhostComponentField[] {
        new GhostComponentField {
          name = "Value",
          attribute = new GhostFieldAttribute{Quantization = -1, Interpolate = true}
        }
      },
      entityIndex = 0
    };
    overrides["Unity.Transforms.Rotation"] = new GhostAuthoringComponentEditor.GhostComponent {
      name = "Unity.Transforms.Rotation",
      attribute = new GhostComponentAttribute { PrefabType = GhostPrefabType.All, OwnerPredictedSendType = GhostSendType.All, SendDataForChildEntity = false },
      fields = new GhostComponentField[] {
        new GhostComponentField {
          name = "Value",
          attribute = new GhostFieldAttribute{Quantization = -1, Interpolate = true}
        }
      },
      entityIndex = 0
    };
  }

  public void ModifyAlwaysIncludedAssembly(HashSet<string> alwaysIncludedAssemblies) {
    alwaysIncludedAssemblies.Add("Unity.Transforms.Translation");
    alwaysIncludedAssemblies.Add("Unity.Transforms.Rotation");
  }

  public void ModifyTypeRegistry(TypeRegistry typeRegistry, string netCodeGenAssemblyPath) {
  }
}