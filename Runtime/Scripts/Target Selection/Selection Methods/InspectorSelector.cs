/*
 * HRTK: InspectorSelector.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

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