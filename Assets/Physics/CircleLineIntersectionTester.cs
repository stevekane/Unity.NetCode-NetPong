using UnityEngine;
using static PhysicsCollisionUtils;

public class CircleLineIntersectionTester : MonoBehaviour {
  public Transform t0;
  public Transform t1;
  public float Radius;
  public float minRadians;
  public float maxRadians;
  public float targetRadians;

  public Transform OutSideSphereChecker;

  void OnDrawGizmos() {
    DrawCircleLineIntersectionGizmos();
    DrawOutsideSphereGizmos();
    DrawWithinArcSegmentGizmos();
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

  void DrawWithinArcSegmentGizmos() {
    var pMin = new Vector3(Mathf.Cos(minRadians) * Radius, 0, Mathf.Sin(minRadians) * Radius);
    var pMax = new Vector3(Mathf.Cos(maxRadians) * Radius, 0, Mathf.Sin(maxRadians) * Radius);
    var p = new Vector3(Mathf.Cos(targetRadians) * Radius, 0, Mathf.Sin(targetRadians) * Radius);
    var color = WithinArcSegment(targetRadians, minRadians, maxRadians) ? Color.green : Color.red;

    Debug.DrawLine(Vector3.zero, pMin, Color.yellow);
    Debug.DrawLine(Vector3.zero, pMax, Color.yellow);
    Debug.DrawLine(Vector3.zero, p, color);
  }
}
