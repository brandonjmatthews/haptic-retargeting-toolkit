using UnityEditor;
using UnityEngine;

namespace HRTK
{

    /// <summary>
    /// Adds a [ReadOnly(string)] attribute that will cause tagged fields to be drawn
    /// with ReadOnlyDrawer in the Inspector, preventing them from being edited. 
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        /// <summary>
        /// String to override the default label with.
        /// </summary>
        public string label;

        /// <summary>
        /// Constructor. Called by the [ReadOnly(string)] tag with parameter in the parenthesis. 
        /// </summary>
        /// <param name="label"></param>
        public ReadOnlyAttribute(string label)
        {
            this.label = label;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom property drawer for fields with a [ReadOnly(string)] attribute
    /// that displays an uneditable text field in the Inspector. 
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            var propertyAttribute = this.attribute as ReadOnlyAttribute;
            label.text = propertyAttribute.label;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

    }
#endif

}