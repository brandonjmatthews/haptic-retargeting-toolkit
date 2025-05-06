#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HRTK.OculusQuest {
    [CustomEditor(typeof(OculusRetargetingHandRig))]
    public class OculusRetargetingHandRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            OculusRetargetingHandRig rig = (OculusRetargetingHandRig)target;

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