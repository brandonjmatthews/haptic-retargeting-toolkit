/*
 * HRTK: InspectorSelectorEditor.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HRTK {
    [CustomEditor(typeof(InspectorSelector))]
    public class InspectorSelectorEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            InspectorSelector selector = (InspectorSelector)target;

            if (selector.VirtualTargets != null) {
                foreach(VirtualTarget virtualTarget in selector.VirtualTargets) {
                    if (virtualTarget != null) {
                        if(GUILayout.Button(virtualTarget.name))
                        {
                            selector.TargetSelected(virtualTarget);
                        }
                    }
                }
            }
        }
    }
}
#endif