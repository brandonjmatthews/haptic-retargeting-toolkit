using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HRTK
{
    public static class Util
    {
        public static Vector3 RelativePoint(Transform transform, Vector3 position) {
            Vector3 relativePoint = position - transform.position;
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(transform.rotation).inverse;
            relativePoint = rotationMatrix.MultiplyPoint3x4(relativePoint);
            return relativePoint;
        }

        public static float ArrayMin(float[] input)
        {
            if (input.Length == 0) return float.NaN;
            float min = input[0];

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] < min)
                {
                    min = input[i];
                }
            }

            return min;
        }

        #region Enums
        public static IEnumerable<T> GetValuesIterable<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static Dictionary<string, int> GetDictionary<T>()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();

            string[] names = Enum.GetNames(typeof(T));

            for (int i = 0; i < names.Length; i++)
            {
                int value = (int)Enum.Parse(typeof(T), names[i]);
                dict.Add(names[i], value);
            }

            return dict;
        }
        #endregion

        #region Math
        public static float MapValue(float aMin, float aMax, float bMin, float bMax, float val)
        {
            return bMin + (bMax - bMin) * ((val - aMin) / (aMax - aMin));
        }

        /// <summary>
        /// Calculates the square of the given float
        /// </summary>
        /// <param name="val">float to be squared</param>
        /// <returns></returns>
        public static float Sq(float a)
        {
            return a * a;
        }

        public static float SmoothStep(float from, float to, float t)
        {
            // Scale, and clamp t to 0..1 range
            t = Mathf.Clamp((t - from) / (to - from), 0.0f, 1.0f);
            // Evaluate polynomial
            return t * t * (3.0f - 2.0f * t);
        }

        public static float SmootherStep(float from, float to, float t)
        {
            // Scale, and clamp t to 0..1 range
            t = Mathf.Clamp((t - from) / (to - from), 0.0f, 1.0f);
            // Evaluate polynomial
            return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
        }

        public static float SmoothestStep(float from, float to, float t)
        {
            // Scale, and clamp t to 0..1 range
            t = Mathf.Clamp((t - from) / (to - from), 0.0f, 1.0f);
            // Evaluate polynomial
            return -20.0f * Mathf.Pow(t, 7.0f) + 70.0f * Mathf.Pow(t, 6.0f) - 84.0f * Mathf.Pow(t, 5.0f) + 35.0f * Mathf.Pow(t, 4.0f);
        }

        /// <summary>
        /// The possible types of sphere intersection
        /// </summary>
        public enum SphereIntersection
        {
            None,
            Tangent,
            Secant
        }

        /// <summary>
        /// Calculates the type and positions of sphere intersections relative to the centre of the sphere
        /// </summary>
        /// <param name="pA">point A on given line</param>
        /// <param name="pB">point B on given line</param>
        /// <param name="o">centre of the target sphere</param>
        /// <param name="r">radius of the target sphere</param>
        /// <param name="iA">first intersection point (or point of tangential intersection)</param>
        /// <param name="iB">second intersection point (only written if secant intersection)</param>
        /// <returns>type of sphere intersection</returns>
        public static SphereIntersection SphereIntersect(Vector3 pA, Vector3 pB, Vector3 o, float r, out Vector3 iA, out Vector3 iB)
        {
            float dx = pB.x - pA.x;
            float dy = pB.y - pA.y;
            float dz = pB.z - pA.z;

            float a = Sq(dx) + Sq(dy) + Sq(dz);
            float b = 2.0f * (dx * (pA.x - o.x) + dy * (pA.y - o.y) + dz * (pA.z - o.z));
            float c = Sq(o.x) + Sq(o.y) + Sq(o.z)
                        + Sq(pA.x) + Sq(pA.y) + Sq(pA.z)
                        - 2.0f * ((o.x * pA.x) + (o.y * pA.y) + (o.z * pA.z))
                        - Sq(r);

            float bb4ac = Sq(b) - 4.0f * a * c;

            if (bb4ac < 0.0f)
            {
                iA = Vector3.zero;
                iB = Vector3.zero;
                return SphereIntersection.None;
            }

            float muA = (-b + Mathf.Sqrt(bb4ac)) / (2.0f * a);
            float muB = (-b - Mathf.Sqrt(bb4ac)) / (2.0f * a);

            iA = pA + muA * (pB - pA);
            iB = pA + muB * (pB - pA);

            if (bb4ac == 0.0f)
            {
                return SphereIntersection.Tangent;
            }
            else
            {
                return SphereIntersection.Secant;
            }
        }
        #endregion

        #region Vectors

        public static float CalculateAngle(Vector3 o, Vector3 a, Vector3 b)
        {
            Vector3 A = a - o;
            Vector3 B = b - o;
            return Vector3.Angle(A, B);
        }

        public static float CalculateTranslation(Vector3 o, Vector3 a, Vector3 b)
        {
            Vector3 A = a - o;
            Vector3 B = b - o;

            float difference = Mathf.Abs(A.magnitude - B.magnitude);
            float gain = 0;

            if (B.magnitude != 0)
            {
                gain = difference / B.magnitude;
                gain = gain * 100;
            }

            return gain;
        }

        public static float CalculateAngle(Vector3 a0, Vector3 b0, Vector3 a1, Vector3 b1)
        {
            Vector3 A = a1 - a0;
            Vector3 B = b1 - b0;
            return Vector3.Angle(A, B);
        }


        public static float CalculateTranslation(Vector3 a0, Vector3 b0, Vector3 a1, Vector3 b1)
        {
            Vector3 A = a1 - a0;
            Vector3 P = b1 - b0;

            float difference = Mathf.Abs(A.magnitude - P.magnitude);
            float gain = 0;

            if (P.magnitude != 0)
            {
                gain = difference / P.magnitude;
            }

            return gain;
        }

        public static float Dot(Vector3 origin, Vector3 a1, Vector3 b1)
        {
            Vector3 vDirection = a1 - origin;
            Vector3 pDirection = b1 - origin;
            return Vector3.Dot(vDirection, pDirection);
        }

        public static float Dot(Vector3 a0, Vector3 b0, Vector3 a1, Vector3 b1)
        {
            Vector3 vDirection = a1 - a0;
            Vector3 pDirection = b1 - b0;
            return Vector3.Dot(vDirection, pDirection);
        }

        public static float DistanceAlongVector(Vector3 origin, Vector3 point, Vector3 direction)
        {
            Vector3 originToPoint = point - origin;
            Vector3 projection = Vector3.Project(originToPoint, direction);
            float distance = projection.magnitude;

            Debug.DrawRay(origin, direction);
            Debug.DrawRay(origin, projection, Color.magenta);
            // float distance = Vector3.Dot(direction, b) - Vector3.Dot(direction, a);
            return distance;
        }
        public static Vector3 GetMidpoint(Vector3 a, Vector3 b) {
            return (a + b) / 2;
        }


        #endregion

        #region Floats
        public static float ScaleBetween(float num, float newMin, float newMax, float oldMin, float oldMax)
        {
            return (newMax - newMin) * (num - oldMin) / (oldMax - oldMin) + newMin;
        }
        #endregion

        #region Debugging
        public static void DrawRay(Ray ray, Color color, float duration = 0.0f, bool depthTest = true)
        {
            Debug.DrawRay(ray.origin, ray.direction, color, duration, depthTest);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Material mat, Color color)
        {
            // set the current material
            mat.SetColor("_Color", color);
            mat.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Vertex3(start.x, start.y, start.z);
            GL.Vertex3(end.x, end.y, end.z);

            GL.End();
        }
        #endregion

        #region Layer Handling
        public static void SetLayerRecursively(GameObject obj, LayerMask newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        #if UNITY_EDITOR
        public static void CreateLayer(string layerName)
        {
            // https://forum.unity.com/threads/adding-layer-by-script.41970/#post-3269876
            if (string.IsNullOrEmpty(layerName))
                throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

            var tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var propCount = layerProps.arraySize;

            UnityEditor.SerializedProperty firstEmptyProp = null;

            for (var i = 0; i < propCount; i++)
            {
                var layerProp = layerProps.GetArrayElementAtIndex(i);

                var stringValue = layerProp.stringValue;

                if (stringValue == layerName) return;

                if (i < 8 || stringValue != string.Empty) continue;

                if (firstEmptyProp == null)
                    firstEmptyProp = layerProp;
            }

            if (firstEmptyProp == null)
            {
                UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + layerName + "\" not created.");
                return;
            }

            firstEmptyProp.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }
        #endif

        #endregion

        #region "Quaternions"
        public static Quaternion ClampQuaternion(Quaternion q, float minAngle, float maxAngle) {
            float currentAngle = Quaternion.Angle(Quaternion.identity, q);

            if (currentAngle < minAngle) {
                return Quaternion.RotateTowards(Quaternion.identity, q, minAngle);
            }

            if (currentAngle > maxAngle) {
                return Quaternion.RotateTowards(Quaternion.identity, q, maxAngle);
            }

            return q;
        }

        #endregion
    }
}