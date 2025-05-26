// float Sphere(Vector3 p, float radius)
// {
//     return p.magnitude - radius;
// }

// float Box(Vector3 p, Vector3 b)
// {
//     b = b / 2.0f;
//     Vector3 d = VectorOps.Abs(p) - b;
//     return VectorOps.Max(d, Vector3.zero).magnitude + VectorOps.MaxElement(VectorOps.Min(d, Vector3.zero));
// }

// float Cylinder(Vector3 p, float r, float h)
// {
//     float d = new Vector2(p.x, p.z).magnitude - r;
//     d = Mathf.Max(d, Mathf.Abs(p.y) - h);
//     return d;
// }

// float Plane(Vector3 p, Vector3 n, float distanceFromOrigin)
// {
//     return Vector3.Dot(p, n) + distanceFromOrigin;
// }

// float Capsule(Vector3 p, Vector3 a, Vector3 b, float r) {
//     return LineSegment(p, a, b) - r;
// }



// float Cone(Vector3 p, float r, float h)
// {
//     Vector2 q = new Vector2(p.xz().magnitude, p.y);
//     Vector2 tip = q - new Vector2(0, h);
//     Vector2 mantleDir = new Vector2(h, r).normalized;
//     float mantle = Vector2.Dot(tip, mantleDir);
//     float d = Mathf.Max(mantle, -q.y);
//     float projected = Vector2.Dot(tip, new Vector2(mantleDir.y, -mantleDir.x));

//     // distance to tip
//     if ((q.y > h) && (projected < 0))
//     {
