// Mesh and Skinned Mesh Distance Compute Helpers
// Author: Brandon Matthews, 2021

struct DistanceResult {
  float3 pointA;
  float3 pointB;
  float distance;
  int intersecting;
};

struct Weight {
  int boneIndex0;
  float weight0;
  int boneIndex1;
  float weight1;
  int boneIndex2;
  float weight2;
  int boneIndex3;
  float weight3;
};

struct ComputePrimitive {
  int type;
  float radius;
  float extraFloat;
  float3 position;
  float3 extraVector;
  float4x4 transform;
};

DistanceResult lerpResult(DistanceResult a, DistanceResult b, float t) {
  DistanceResult r;
  r.pointA = lerp(a.pointA, b.pointA, t);
  r.pointB = lerp(a.pointB, b.pointB, t);
  r.distance = lerp(a.distance, b.distance, t);

  return r;
}

/* Returns the normal of the input triangle. */
float3 tri_normal(float3 v0, float3 v1, float3 v2) {
  float3 e10 = v1 - v0;
  float3 e21 = v2 - v1;
  float3 e02 = v0 - v2;
  return normalize(cross(e10, e02));
}


// Compute minimum distance between two segment
DistanceResult segment_to_segment(float3 segment0[2], float3 segment1[2]) {
  float3 dir0 = segment0[1] - segment0[0];
  float3 dir1 = segment1[1] - segment1[0];

  float3 lineDiff = segment0[0] - segment1[0];

  float a = dot(dir0, dir0);
  float e = dot(dir1, dir1);
  float f = dot(dir1, lineDiff);
  float c = dot(dir0, lineDiff);
  float b = dot(dir0, dir1);

  // s and t are the parameter values from iLine1and iLine2.
  float denom = a * e - b * b;
  // denom = max(denom, eps(e))?
  float s = clamp((b * f - c * e) / denom, 0, 1);
  // e = max(e, eps(e));
  float t = (b * s + f) / e;

  // If t in [0,1] done. Else clamp t, recompute s for the new value of t and
  // clamp s to [0, 1]
  float newT = clamp(t, 0, 1);

  // float diff = abs(newT-t);
  // int newTNotEqualT = step(diff, 0);
  // float newS = clamp((newT * b - c) / a, 0, 1);
  // s = lerp(s, newS, newTNotEqualT);

  if (newT != t) {
    s = clamp((newT * b - c) / a, 0, 1);
  }

  // Compute closest points and return distance
  DistanceResult result;
  result.pointA = segment0[0] + dir0 * s;
  result.pointB = segment1[0] + dir1 * newT;
  result.distance = length(result.pointA - result.pointB);
  result.intersecting = 0;
  return result;
}

// Find a direction that demonstrates that the current side is closest and
// separates the triangles.
bool closest_edge_points(float3 triPoint1, float3 triClosePoint1,
                        float3 triPoint2, float3 triClosePoint2,
                        float3 sepDir) {
  float3 awayDir = triPoint1 - triClosePoint1;
  float diffDir = dot(awayDir, sepDir);

  awayDir = triPoint2 - triClosePoint2;
  float sameDir = dot(awayDir, sepDir);

  return diffDir <= 0 && sameDir >= 0;
}

// Compute the distance between a triangle edge and another triangle’s edges
void closest_edge_to_edge(float3 tri1Edges[3][2], float3 tri2Edge[2],
                          float3 tri2LastPoint, inout bool finished,
                          out DistanceResult result) {
  DistanceResult seg0 = segment_to_segment(tri1Edges[0], tri2Edge);
  float3 sepDir = seg0.pointB - seg0.pointA;
  finished = finished || closest_edge_points(tri1Edges[1][0], seg0.pointA,
                                             tri2LastPoint, seg0.pointB, sepDir);

  result.pointA = seg0.pointA;
  result.pointB = seg0.pointB;
  result.intersecting = 0;

  if (finished) {
    result.distance = seg0.distance;
    return;
  }

  DistanceResult seg1 = segment_to_segment(tri1Edges[1], tri2Edge);
  sepDir = seg1.pointB - seg1.pointA;
  finished = finished || closest_edge_points(tri1Edges[2][0], seg1.pointA,
                                             tri2LastPoint, seg1.pointB, sepDir);

  float ABdist;
  if (seg0.distance < seg1.distance) {
    ABdist = seg0.distance;
    result.pointA = seg0.pointA;
    result.pointB = seg0.pointB;
  } else {
    ABdist = seg1.distance;
    result.pointA = seg1.pointA;
    result.pointB = seg1.pointB;
  }

  if (finished) {
    result.distance = ABdist;
    return;
  }

  DistanceResult seg2 = segment_to_segment(tri1Edges[2], tri2Edge);
  sepDir = seg2.pointB - seg2.pointA;
  finished = finished || closest_edge_points(tri1Edges[0][0], seg2.pointA,
                                             tri2LastPoint, seg2.pointB, sepDir);

  if (ABdist < seg2.distance) {
    result.distance = ABdist;
  } else {
    result = seg2;
    result.pointA = seg2.pointA;
    result.pointB = seg2.pointB;
  }
}

float3 project_on_line(float3 start, float3 end, float3 p) {
  float3 lineVec = end - start;
  float3 pointVec = p - start;

  float t = dot(normalize(pointVec), normalize(lineVec));

  float3 proj = start + dot(pointVec, lineVec) / dot(lineVec, lineVec) * lineVec;

  float3 projVec = start - proj;
  
  // int tGreaterThanZero = step(0, t);
  // int projLessThanLine = step(length(lineVec), length(projVec));
  
  // float3 result = lerp(end, proj, projLessThanLine);
  // result = lerp(start, result, tGreaterThanZero);
  // return result;
  if (t > 0) {
    if (length(projVec) <= length(lineVec)) {
      return proj;
    } else {
      return end;
    }
  } else {
    return start;
  }
  // int afterStart = step(t, 0);
  // int onSegment = step(length(pointVSeemsec), length(lineVec));
  // float3 pos = lerp(start, lerp(end, proj, onSegment), afterStart);

  // return pos;
}

void tri_closest_point(float3 tri[3], float3 p, out float dist,
                       out float3 oPoint) {
	float3 ba = tri[1] - tri[0]; // u
	float3 ca = tri[2] - tri[0]; // v
	float3 cb = tri[2] - tri[1]; // b
	float3 nor = cross(ba, ca);
	float3 pa = p - tri[0];

	// Compute barycentric coordinates
	float w = dot(cross(ba, pa), nor) / dot(nor, nor); // gamma
	float v = dot(cross(pa, ca), nor) / dot(nor, nor); // beta
	float u = 1 - w - v;                               // alpha

	// float d = 1 / 3;
	// // Get barycentric center
	// float3 center = d * a + d * b + d * c;
	// Get barycentric in cartesian space
	float3 triProj = u * tri[0] + v * tri[1] + w * tri[2];

	// check if in range
	bool uInside = 0 <= u && u <= 1;
	bool vInside = 0 <= v && v <= 1;
	bool wInside = 0 <= w && w <= 1;

	bool inside = uInside && vInside && wInside;

	if (inside) {
    oPoint = triProj;
    dist = length(p - oPoint);
    return;
  }

	float3 proj01 = project_on_line(tri[0], tri[1], triProj);
	float3 proj21 = project_on_line(tri[2], tri[1], triProj);
	float3 proj20 = project_on_line(tri[2], tri[0], triProj);

	float dist01 = length(p - proj01);
	float dist21 = length(p - proj21);
	float dist20 = length(p - proj20);

	// float3 minDistProj; // lerp(lerp(proj20, proj21, proj21Min), proj01, proj01Min);
	if (dist21 > dist01 && dist20 > dist01) {
		oPoint = proj01;
	} else if (dist01 > dist21 && dist20 > dist21) {
		oPoint = proj21; 
	} else {
		oPoint = proj20;
	}

	dist = length(p - oPoint);
	return;
}

// Compute the distance between iTriB vertexes and another triangle iTriA
void closest_vert_to_tri(float3 triA[3], float3 triB[3],
                         out DistanceResult result) {
  float A, B, C;
  float3 Ap, Bp, Cp;

  tri_closest_point(triA, triB[0], A, Ap);
  tri_closest_point(triA, triB[1], B, Bp);
  tri_closest_point(triA, triB[2], C, Cp);

  float ABdist;
  float3 ABp;
  if (A < B) {
    ABdist = A;
    ABp = Ap;
  } else {
    ABdist = B;
    ABp = Bp;
  }

  if (ABdist < C) {
    result.pointA = ABp;

    if (A < B) {
      result.pointB = triB[0];
    } else {
      result.pointB = triB[1];
    }

    result.distance = ABdist;
    result.intersecting = 0;
  } else {
    result.pointA = Cp;
    result.pointB = triB[2];
    result.distance = C;
    result.intersecting = 0;
  }
}

// A common subroutine for each separating direction
int project(float3 ax, float3 p1, float3 p2, float3 p3, float3 q1, float3 q2,
            float3 q3) {

  float P1 = dot(ax, p1);
  float P2 = dot(ax, p2);
  float P3 = dot(ax, p3);

  float Q1 = dot(ax, q1);
  float Q2 = dot(ax, q2);
  float Q3 = dot(ax, q3);

  float mx1 = max(P1, max(P2, P3));
  float mn1 = min(P1, min(P2, P3));
  float mx2 = max(Q1, max(Q2, Q3));
  float mn2 = min(Q1, min(Q2, Q3));

  return (mn1 <= mx2) && (mn2 <= mx1);
}

int tri_contact(float3 triA[3], float3 triB[3]) {
  float3 P1 = triA[0];
  float3 P2 = triA[1];
  float3 P3 = triA[2];

  float3 Q1 = triB[0];
  float3 Q2 = triB[1];
  float3 Q3 = triB[2];

  float3 p1 = float3(0, 0, 0);
  float3 p2 = P2 - P1;
  float3 p3 = P3 - P1;

  float3 q1 = Q1 - P1;
  float3 q2 = Q2 - P1;
  float3 q3 = Q3 - P1;

  float3 e1 = P2 - P1;
  float3 e2 = P3 - P2;

  float3 f1 = Q2 - Q1;
  float3 f2 = Q3 - Q2;

  int mask = 1;

  float3 n1 = cross(e1, e2);
  int x0 = project(n1, p1, p2, p3, q1, q2, q3);
  float3 m1 = cross(f1, f2);
  int x1 = project(m1, p1, p2, p3, q1, q2, q3);

  float3 t;
  t = cross(e1, f1);
  int x2 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e1, f2);
  int x3 = project(t, p1, p2, p3, q1, q2, q3);
  float3 f3 = q1 - q3;
  t = cross(e1, f3);
  int x4 = project(t, p1, p2, p3, q1, q2, q3);

  t = cross(e2, f1);
  int x5 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e2, f2);
  int x6 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e2, f3);
  int x7 = project(t, p1, p2, p3, q1, q2, q3);

  float3 e3 = p1 - p3;
  t = cross(e3, f1);
  int x8 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e3, f2);
  int x9 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e3, f3);
  int x10 = project(t, p1, p2, p3, q1, q2, q3);

  t = cross(e1, n1);
  int x11 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e2, n1);
  int x12 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(e3, n1);
  int x13 = project(t, p1, p2, p3, q1, q2, q3);

  t = cross(f1, m1);
  int x14 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(f2, m1);
  int x15 = project(t, p1, p2, p3, q1, q2, q3);
  t = cross(f3, m1);
  int x16 = project(t, p1, p2, p3, q1, q2, q3);

  return x1 && x2 && x3 && x4 && x5 && x6 && x7 && x8 && x9 && x10 && x11 &&
         x12 && x13 && x14 && x15 && x16;
}


DistanceResult LineToLine(float3 a0, float3 a1, float3 b0, float3 b1) {
  float3 A = a1 - a0;
  float3 B = b1 - b0;

  float3 diff = a0 - b0;

  float a = dot(A, A);
  float b = dot(B, B);
  float c = dot(A, B);
  float e = dot(B, diff);
  float f = dot(A, diff);

  float d = a * b - c * c;
  float s = clamp((c * e - f * b) / d, 0.0, 1.0);
  float t = (c * s + e) / b;

  float nt = clamp(t, 0.0, 1.0);

  if (nt != t) {
    s = clamp((nt * c - f) / a, 0.0, 1.0);
  }

  DistanceResult result;
  result.pointA = a0 + A * s;
  result.pointB = b0 + B * nt;

  result.distance = distance(result.pointA, result.pointB);
  
  return result;
}

DistanceResult LineToTriangle(float3 a0, float3 a1, float3 b0, float3 b1, float3 b2) {
  float3 s01 = b0 - b1;
  float3 s02 = b0 - b2;

  float3 n = cross(s01, s02);

  float3 oa0 = a0 - b0;
  float3 oa1 = a1 - b0;
  

  float p0 = n.x * oa0.x + n.y * oa0.y + n.z * oa0.z;
  float p1 = n.x * oa1.x + n.y * oa1.y + n.z * oa1.z;

  float s0 = sign(p0);
  float s1 = sign(p1);

  //int intersecting = s0 != 0 && s1 != 0 && s0 != s1;

  DistanceResult r1 = LineToLine(a0, a1, b0, b1);
  DistanceResult r2 = LineToLine(a0, a1, b1, b2);
  DistanceResult r3 = LineToLine(a0, a1, b2, b0);

  DistanceResult r;

  if (r1.distance < r2.distance && r1.distance < r3.distance) r = r1;
  else if (r2.distance < r1.distance && r2.distance < r3.distance) r = r2;
  else r = r3;

  r.intersecting = 0;
  return r;
}