using System.Collections.Generic;
using UnityEngine;

namespace HRTK
{
    public class JointRetargetingShape : PointRetargetingShape
    {
        public Transform TrackingRoot;
        private void Awake() {
            Points = new List<Transform>(TrackingRoot.GetComponentsInChildren<Transform>());
        }
    }
}