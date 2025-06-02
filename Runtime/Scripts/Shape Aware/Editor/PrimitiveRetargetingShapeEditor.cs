/*
 * HRTK: PrimitiveRetargetingShapeEditor.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using UnityEngine;
using UnityEditor;

namespace HRTK.Modules.ShapeRetargeting {
    [CustomEditor(typeof(PrimitiveRetargetingShape))]
    public class PrimitiveRetargetingShapeEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            PrimitiveRetargetingShape shape = (PrimitiveRetargetingShape)target;

            if (GUILayout.Button("Add Primitive Point")) 
                shape.Primitives.Add(new GameObject("Primitive Point").AddComponent<PrimitivePoint>());

            if (GUILayout.Button("Add Primitive Sphere"))
                shape.Primitives.Add(new GameObject("Primitive Sphere").AddComponent<PrimitiveSphere>());

            if (GUILayout.Button("Add Primitive Box")) 
                shape.Primitives.Add(new GameObject("Primitive Box").AddComponent<PrimitiveBox>());

            if (GUILayout.Button("Add Primitive Cylinder")) 
                shape.Primitives.Add(new GameObject("Primitive Cylinder").AddComponent<PrimitiveCylinder>());

            if (GUILayout.Button("Add Primitive Capsule")) 
                shape.Primitives.Add(new GameObject("Primitive Capsule").AddComponent<PrimitiveCapsule>());
        }
    }
}