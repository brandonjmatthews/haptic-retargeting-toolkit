/*
 * HRTK: Primitive.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using System.Collections.Generic;
using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public class LineSegment
    {
        public Vector3 Start;
        public Vector3 End;
        public bool IsPoint = false;
    }


    public abstract class Primitive : MonoBehaviour
    {
        // public bool ShowAtRuntime;
        // public GameObject LinePrefab;
        // public float LineWidth;

        // protected virtual void Start() {
        //     if (ShowAtRuntime) Draw();
        // }
        public bool invert;
        
        public abstract DistanceResult Distance(Vector3 other);
        public abstract DistanceResult Distance(Primitive other);

        public abstract float SignedDistance(Vector3 position);
        public virtual Vector3 RelativePoint(Vector3 point) {
            Vector3 relativePoint = point - transform.position;
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(transform.rotation).inverse;

            relativePoint = rotationMatrix.MultiplyPoint3x4(relativePoint);
            return relativePoint;
        }

        public Matrix4x4 Transformation() {
            return Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        }
    }   
}