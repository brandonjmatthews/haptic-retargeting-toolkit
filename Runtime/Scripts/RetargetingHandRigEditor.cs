/*
 * HRTK: RetargetingHandRigEditor.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HRTK {
    [CustomEditor(typeof(RetargetingHandRig))]
    public class RetargetingHandRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            RetargetingHandRig rig = (RetargetingHandRig)target;


            EditorGUI.BeginDisabledGroup(!PrefabUtility.IsPartOfNonAssetPrefabInstance(rig));
            if (GUILayout.Button("Build Hand Rig"))
            {
                rig.BuildRig();
            }

            if (GUILayout.Button("Clear Hand Rig"))
            {
                rig.ClearRig();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif