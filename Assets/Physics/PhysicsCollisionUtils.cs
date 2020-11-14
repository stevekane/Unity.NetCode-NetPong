using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class PhysicsCollisionUtils {
  public static readonly float TWO_PI = 2 * PI;

  public static float2 FromXZPlane(in float3 p) {
    return float2(p.x, p.z);
  }

  public static float3 ToXZPlane(in float2 p) {
    return float3(p.x, 0, p.y);
  }

  public static bool PointIsOnLineSegment(float2 p1, float2 p2, float2 p) {
    var xHigh = max(p1.x, p2.x);
    var xLow = min(p1.x, p2.x);
    var yHigh = max(p1.y, p2.y);
    var yLow = min(p1.y, p2.y);

    return p.x < xHigh && p.x > xLow && p.y < yHigh && p.y >= yLow;
  }

  public static int LineSegmentCircleIntersection(float2 p1, float2 p2, float r, ref float2[] points) {
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
    var sqrtDelta = sqrt(delta);

    if (delta > 0) {
      var x_1 = (D * dy + sgn(dy) * dx * sqrtDelta) / (drSquared);
      var x_2 = (D * dy - sgn(dy) * dx * sqrtDelta) / (drSquared);
      var y_1 = (-D * dx + abs(dy) * sqrtDelta) / (drSquared);
      var y_2 = (-D * dx - abs(dy) * sqrtDelta) / (drSquared);        
      var i1 = new float2(x_1, y_1);
      var i2 = new float2(x_2, y_2);
      var i1Valid = PointIsOnLineSegment(p1, p2, i1);
      var i2Valid = PointIsOnLineSegment(p1, p2, i2);
      var pointIndex = 0;

      if (i1Valid) {
        points[pointIndex++] = i1; 
      }
      if (i2Valid) {
        points[pointIndex++] = i2;
      }
      return pointIndex;
    } else {
      return 0;
    }
  }

  public static bool LineSegmentCircleIntersectionAPPROXIMATE(float2 p1, float2 p2, float r, out float2 point) {
    var center = float2(0,0);

    if (PointOutsideCircle(p1, center, r)) {
      point = float2(0,0);
      return false;
    }

    if (!PointOutsideCircle(p2, center, r)) {
      point = float2(0,0);
      return false;
    }

    var lengthOutsideRadius = PointOutsideCircleDistance(p2, center, r);
    var delta = p2 - p1;
    var totalDistance = length(delta);
    var direction = delta / totalDistance;
    var distanceToImpact = totalDistance - lengthOutsideRadius;

    point = p1 + direction * distanceToImpact;
    return true;
  }

  public static float2 ReflectAbout(float2 d, float2 n) {
    return n - 2 * dot(d, n) * n;
  }

  public static float AddRadians(float r0, float dr) {
    var r1 = (r0 + dr) % TWO_PI; 

    return select(r1, r1 + TWO_PI, r1 < 0);
  }

  public static float CartesianToRadians(float2 p) {
    var radians = atan2(p.y, p.x);

    return (radians > 0) ? (radians) : (radians + TWO_PI);
  }

  public static bool PointOutsideCircle(float2 p, float2 center, float radius) {
    return lengthsq(p - center) > (radius * radius);
  }

  public static float PointOutsideCircleDistance(float2 p, float2 center, float radius) {
    return length(p - center) - radius;
  }

  // Assumes all values between 0-2pi
  public static bool WithinArcSegment(float targetRadians, float minRadians, float maxRadians) {
    if (minRadians <= maxRadians) {
      if (maxRadians - minRadians <= PI) {
        return minRadians <= targetRadians && targetRadians <= maxRadians;
      } else {
        return maxRadians <= targetRadians || targetRadians <= minRadians;
    }
    } else {
      if (minRadians - maxRadians <= PI) {
        return maxRadians <= targetRadians && targetRadians <= minRadians;
      } else {
        return minRadians <= targetRadians || targetRadians <= maxRadians;
      }
    }
  }
}