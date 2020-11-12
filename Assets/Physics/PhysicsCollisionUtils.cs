using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class PhysicsCollisionUtils {
  public static bool PointIsOnLineSegment(float2 p1, float2 p2, float2 p) {
    var xHigh = max(p1.x, p2.x);
    var xLow = min(p1.x, p2.x);
    var yHigh = max(p1.y, p2.y);
    var yLow = min(p1.y, p2.y);

    return p.x < xHigh && p.x > xLow && p.y < yHigh && p.y >= yLow;
  }

  public static (float2?, float2?) LineSegmentCircleIntersection(float2 p1, float2 p2, float r) {
    float sgn(float x) => x < 0 ? -1 : 1;

    var x1 = p1.x;
    var y1 = p1.y;
    var x2 = p2.x;
    var y2 = p2.y;
    var dx = x2 - x1;
    var dy = y2 - y1;
    var drSquared = dx * dx + dy * dy;
    var D = x1 * y2 - x2 * y1;
    var delta = r * r * drSquared - D * D;

    if (delta > 0) {
      var x_1 = (D * dy + sgn(dy) * dx * sqrt(delta)) / (drSquared);
      var x_2 = (D * dy - sgn(dy) * dx * sqrt(delta)) / (drSquared);
      var y_1 = (-D * dx + abs(dy) * sqrt(delta)) / (drSquared);
      var y_2 = (-D * dx - abs(dy) * sqrt(delta)) / (drSquared);        
      var i1 = new float2(x_1, y_1);
      var i2 = new float2(x_2, y_2);
      var i1Valid = PointIsOnLineSegment(p1, p2, i1);
      var i2Valid = PointIsOnLineSegment(p1, p2, i2);
      float2? out1 = i1Valid ? i1 : (float2?)null;
      float2? out2 = i2Valid ? i2 : (float2?)null;

      return (out1, out2);
    } else {
      return (null, null);
    }
  }

  public static bool PointOutsideCircle(float2 p, float2 center, float radius) {
    return lengthsq(p - center) > radius * radius;
  }

  public static float PointOutsideCircleDistance(float2 p, float2 center, float radius) {
    return length(p - center) - radius;
  }
}