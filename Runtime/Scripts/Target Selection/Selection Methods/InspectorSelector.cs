using UnityEngine;
using UnityEngine.Events;

namespace HRTK {
    public class InspectorSelector : TargetSelector
    {
        public bool LoadTargetsOnStart;
        public VirtualTarget[] VirtualTargets;
        
        void Awake() {
            if (LoadTargetsOnStart) {
                UpdateTargetList();
            }
        }

        public void UpdateTargetList() {
            VirtualTargets = Object.FindObjectsOfType<VirtualTarget>();
        }
    }
}