using UnityEditor;
using UnityEngine;

namespace Dythervin.AutoAttach
{
    [CustomPropertyDrawer(typeof(AttachAttribute), true)]
    public class AutoAttachDrawer : PropertyDrawer
    {
        private AttachAttribute Attribute => (AttachAttribute)attribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool readOnly = Attribute.readOnly;
            if (readOnly)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            EditorGUI.PropertyField(position, property, label, true);

            if (readOnly)
            {
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}