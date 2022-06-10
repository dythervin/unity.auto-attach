#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Dythervin.AutoAttach.Editor
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
            using (new EditorGUI.DisabledScope(Attribute.readOnly))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
#endif