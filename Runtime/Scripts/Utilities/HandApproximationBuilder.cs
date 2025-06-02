/*
 * HRTK: HandApproximationBuilder.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting {
    public class HandApproximationBuilder : MonoBehaviour
    {
        [System.Serializable]
        public class Finger {
            public Transform root;
            public int joints;
            public float radius;
        }

        public Finger[] Fingers;


        PrimitiveRetargetingShape _fieldComputeMethod;
        List<Primitive> _primitives;

        void Start() {
            _fieldComputeMethod = GetComponent<PrimitiveRetargetingShape>();
            _primitives = new List<Primitive>(_fieldComputeMethod.Primitives);
            if (_fieldComputeMethod != null) {
                for (int i = 0; i < Fingers.Length; i++)
                _primitives.AddRange(GenerateFingerPrimitives(Fingers[i]));

                _fieldComputeMethod.Primitives = _primitives;
            }
        }

        Primitive[] GenerateFingerPrimitives(Finger finger) {

            Primitive[] fingerPrimitives = new Primitive[finger.joints];

            Transform currentJoint = finger.root;
            for (int i = 0; i < finger.joints; i++) {
                Transform nextJoint = currentJoint.GetChild(0);

                if (nextJoint == null) {
                    Debug.LogWarningFormat("Missing child joint, check joint number");
                }

                PrimitiveCapsule capsule = currentJoint.GetComponent<PrimitiveCapsule>();
                if (capsule == null) capsule = currentJoint.gameObject.AddComponent<PrimitiveCapsule>();

                capsule.Radius = finger.radius;
                capsule.LocalStartPoint = Vector3.zero;
                capsule.LocalEndPoint = nextJoint.transform.localPosition;
                fingerPrimitives[i] = capsule;

                currentJoint = nextJoint;
            }

            return fingerPrimitives;
        }
    }
}
