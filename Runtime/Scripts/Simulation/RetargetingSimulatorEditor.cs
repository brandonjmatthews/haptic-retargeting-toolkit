#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace HRTK.Simulation
{
    [CustomEditor(typeof(RetargetingSimulator))]
    public class RetargetingSimulatorEditor : Editor
    {
        private SerializedProperty _property;
        private ReorderableList _list;

        private void OnEnable()
        {
            _property = serializedObject.FindProperty("Procedure");
            _list = new ReorderablePolymorphicList(serializedObject, _property, true, true, true, true);
        }

        public override void OnInspectorGUI()
        {
            RetargetingSimulator target = serializedObject.targetObject as RetargetingSimulator;
            serializedObject.Update();
            Editor.DrawPropertiesExcluding(serializedObject, new string[1] {"Procedure"});
            EditorGUILayout.Space();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying) {
                if (!target.IsStarted && GUILayout.Button("Start Procedure")) {
                    target.StartProcedure();
                }

                string playPause = target.IsRunning ? "Pause" : "Play";
                if (target.IsStarted && GUILayout.Button(playPause)) {
                    target.ToggleProcedurePaused();
                }
            }
        }
    }
}
#endif