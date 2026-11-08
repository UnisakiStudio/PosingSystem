using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // Translate display labels without changing the serialized enum names or numeric values.
    public abstract class PosingSystemLocalizedEnumDrawer : PropertyDrawer
    {
        internal static GUIContent[] Labels(SerializedProperty property)
        {
            var names = property.enumDisplayNames;
            var labels = new GUIContent[names.Length];
            for (var i = 0; i < names.Length; i++) labels[i] = new GUIContent(PosingSystemLocalization.Text(names[i]));
            return labels;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = new GUIContent(label) { text = PosingSystemLocalization.Text(label.text), tooltip = PosingSystemLocalization.Text(label.tooltip) };
            EditorGUI.BeginProperty(position, label, property);
            try
            {
                EditorGUI.BeginChangeCheck();
                var selected = EditorGUI.Popup(position, label, property.enumValueIndex, Labels(property));
                if (EditorGUI.EndChangeCheck()) property.enumValueIndex = selected;
            }
            finally { EditorGUI.EndProperty(); }
        }
    }

    [CustomPropertyDrawer(typeof(PosingSystemAutoPoseLimb))]
    public sealed class PosingSystemAutoPoseLimbDrawer : PosingSystemLocalizedEnumDrawer { }
    [CustomPropertyDrawer(typeof(PosingSystemAutoPoseTargetSpace))]
    public sealed class PosingSystemAutoPoseTargetSpaceDrawer : PosingSystemLocalizedEnumDrawer { }
    [CustomPropertyDrawer(typeof(PosingSystemAutoPoseBendDirectionMode))]
    public sealed class PosingSystemAutoPoseBendDirectionDrawer : PosingSystemLocalizedEnumDrawer { }
    [CustomPropertyDrawer(typeof(PosingSystemAutoPosePositionSpace))]
    public sealed class PosingSystemAutoPosePositionSpaceDrawer : PosingSystemLocalizedEnumDrawer { }
    [CustomPropertyDrawer(typeof(PosingSystemAutoPoseLengthReference))]
    public sealed class PosingSystemAutoPoseLengthReferenceDrawer : PosingSystemLocalizedEnumDrawer { }
}
