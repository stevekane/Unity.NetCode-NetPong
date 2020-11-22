using UnityEngine;

public static class ObjectExtensions {
  public static int Hash(this Object o) {
    return Animator.StringToHash(o.name);
  }
}