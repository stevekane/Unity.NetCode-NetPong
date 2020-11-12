using UnityEngine;
using static PhysicsCollisionUtils;

public class CircleLineIntersectionTester : MonoBehaviour {
  public Transform t0;
  public Transform t1;
  public float Radius;

  public Transform OutSideSphereChecker;

  void OnDrawGizmos() {
    DrawCircleLineIntersectionGizmos();
    DrawOutsideSphereGizmos();
  }

  void DrawCircleLineIntersectionGizmos() {
    var p0 = new Vector2(t0.position.x, t0.position.z);
    var p1 = new Vector2(t1.position.x, t1.position.z);
    var intersections = LineSegmentCircleIntersection(p0, p1, Radius);

    if (intersections.Item1.HasValue) {
      var pt = new Vector3(intersections.Item1.Value.x, 0, intersections.Item1.Value.y);

      Debug.DrawLine(Vector3.zero, pt, Color.green);
    }
    if (intersections.Item2.HasValue) {
      var pt = new Vector3(intersections.Item2.Value.x, 0, intersections.Item2.Value.y);

      Debug.DrawLine(Vector3.zero, pt, Color.green);
    }

    Debug.DrawLine(t0.position, t1.position, Color.yellow);
    Gizmos.DrawWireSphere(Vector3.zero, Radius);
  }

  void DrawOutsideSphereGizmos() {
    var p = new Vector2(OutSideSphereChecker.position.x, OutSideSphereChecker.position.z);

    if (PointOutsideCircle(p, Vector2.zero, Radius)) {
      Debug.DrawLine(Vector3.zero, OutSideSphereChecker.position, Color.red);
    } else {
      Debug.DrawLine(Vector3.zero, OutSideSphereChecker.position, Color.green);
    }
  }
}
