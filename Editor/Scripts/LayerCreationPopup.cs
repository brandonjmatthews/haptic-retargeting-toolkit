#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace HRTK {
    [InitializeOnLoad]
    public class LayerCreationPopup : EditorWindow
    {
        #if UNITY_EDITOR
        [MenuItem("HRTK/Add Recommended Layers")]
        public static void InitPopup()
        {
            LayerCreationPopup window = ScriptableObject.CreateInstance<LayerCreationPopup>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Create recommended layers for HRTK", EditorStyles.wordWrappedLabel);

            LayerButton(Constants.TrackedEnvironmentLayer);
            LayerButton(Constants.VirtualEnvironmentLayer);

            if (GUILayout.Button("Cancel")) this.Close();
        }

        void LayerButton(string layer) {
            if (LayerMask.NameToLayer(layer) == -1) {
                if (GUILayout.Button("Create '" + layer + "' Layer")) Util.CreateLayer(layer);
            } else {
                GUILayout.Label(layer + " already exists!");
            }
        }


        #endif
    }
}
#endif